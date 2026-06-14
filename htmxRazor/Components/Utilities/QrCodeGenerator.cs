using System.Text;

namespace htmxRazor.Components.Utilities;

/// <summary>
/// Self-contained QR code generator (no external dependencies). Produces the QR
/// module matrix for byte-mode encoding, versions 1–10, and all error-correction
/// levels (L/M/Q/H). This is a faithful C# port of the previous client-side
/// <c>rhx-qr-code.js</c> algorithm and produces a byte-for-byte identical module
/// matrix for the same input and EC level.
/// </summary>
public static class QrCodeGenerator
{
    // ═══════════════════════════════════════════════
    //  GF(256) arithmetic — primitive polynomial 0x11D
    // ═══════════════════════════════════════════════

    private static readonly byte[] GfExp = new byte[512];
    private static readonly byte[] GfLog = new byte[256];

    static QrCodeGenerator()
    {
        var v = 1;
        for (var i = 0; i < 255; i++)
        {
            GfExp[i] = (byte)v;
            GfLog[v] = (byte)i;
            v = (v << 1) ^ ((v & 128) != 0 ? 0x11d : 0);
        }
        for (var i = 255; i < 512; i++) GfExp[i] = GfExp[i - 255];
    }

    private static int GfMul(int a, int b)
    {
        if (a == 0 || b == 0) return 0;
        return GfExp[GfLog[a] + GfLog[b]];
    }

    // ═══════════════════════════════════════════════
    //  Reed-Solomon error correction
    // ═══════════════════════════════════════════════

    private static int[] RsGenPoly(int n)
    {
        var poly = new[] { 1 };
        for (var i = 0; i < n; i++)
        {
            var next = new int[poly.Length + 1];
            for (var j = 0; j < poly.Length; j++)
            {
                next[j] ^= poly[j];
                next[j + 1] ^= GfMul(poly[j], GfExp[i]);
            }
            poly = next;
        }
        return poly;
    }

    private static int[] RsEncode(int[] data, int eccCount)
    {
        var gen = RsGenPoly(eccCount);
        var result = new int[eccCount];
        for (var i = 0; i < data.Length; i++)
        {
            var coef = data[i] ^ result[0];
            for (var j = 0; j < eccCount - 1; j++) result[j] = result[j + 1];
            result[eccCount - 1] = 0;
            if (coef != 0)
            {
                for (var j = 0; j < eccCount; j++)
                    result[j] ^= GfMul(gen[j + 1], coef);
            }
        }
        return result;
    }

    // ═══════════════════════════════════════════════
    //  QR code data tables
    // ═══════════════════════════════════════════════

    // [eccPerBlock, groups] where groups is a list of (count, dataCW)
    private sealed record EcParams(int EccPerBlock, (int Count, int DataCW)[] Groups);

    private static readonly Dictionary<string, EcParams>?[] EC =
    {
        null,
        /* v1  */ Lvl(("L", 7, new[] { (1, 19) }), ("M", 10, new[] { (1, 16) }), ("Q", 13, new[] { (1, 13) }), ("H", 17, new[] { (1, 9) })),
        /* v2  */ Lvl(("L", 10, new[] { (1, 34) }), ("M", 16, new[] { (1, 28) }), ("Q", 22, new[] { (1, 22) }), ("H", 28, new[] { (1, 16) })),
        /* v3  */ Lvl(("L", 15, new[] { (1, 55) }), ("M", 26, new[] { (1, 44) }), ("Q", 18, new[] { (2, 17) }), ("H", 22, new[] { (2, 13) })),
        /* v4  */ Lvl(("L", 20, new[] { (1, 80) }), ("M", 18, new[] { (2, 32) }), ("Q", 26, new[] { (2, 24) }), ("H", 16, new[] { (4, 9) })),
        /* v5  */ Lvl(("L", 26, new[] { (1, 108) }), ("M", 24, new[] { (2, 43) }), ("Q", 18, new[] { (2, 15), (2, 16) }), ("H", 22, new[] { (2, 11), (2, 12) })),
        /* v6  */ Lvl(("L", 18, new[] { (2, 68) }), ("M", 16, new[] { (4, 27) }), ("Q", 24, new[] { (4, 19) }), ("H", 28, new[] { (4, 15) })),
        /* v7  */ Lvl(("L", 20, new[] { (2, 78) }), ("M", 18, new[] { (4, 31) }), ("Q", 18, new[] { (2, 14), (4, 15) }), ("H", 26, new[] { (4, 13), (1, 14) })),
        /* v8  */ Lvl(("L", 24, new[] { (2, 97) }), ("M", 22, new[] { (2, 38), (2, 39) }), ("Q", 22, new[] { (4, 18), (2, 19) }), ("H", 26, new[] { (4, 14), (2, 15) })),
        /* v9  */ Lvl(("L", 30, new[] { (2, 116) }), ("M", 22, new[] { (3, 36), (2, 37) }), ("Q", 20, new[] { (4, 16), (4, 17) }), ("H", 24, new[] { (4, 12), (4, 13) })),
        /* v10 */ Lvl(("L", 18, new[] { (2, 68), (2, 69) }), ("M", 26, new[] { (4, 43), (1, 44) }), ("Q", 24, new[] { (6, 19), (2, 20) }), ("H", 28, new[] { (6, 15), (2, 16) })),
    };

    private static Dictionary<string, EcParams> Lvl(params (string Level, int Ecc, (int, int)[] Groups)[] entries)
    {
        var dict = new Dictionary<string, EcParams>();
        foreach (var e in entries)
            dict[e.Level] = new EcParams(e.Ecc, e.Groups);
        return dict;
    }

    // Alignment pattern center positions per version
    private static readonly int[][] ALIGN =
    {
        null!, Array.Empty<int>(), new[] { 6, 18 }, new[] { 6, 22 }, new[] { 6, 26 }, new[] { 6, 30 },
        new[] { 6, 34 }, new[] { 6, 22, 38 }, new[] { 6, 24, 42 }, new[] { 6, 26, 46 }, new[] { 6, 28, 52 },
    };

    // Remainder bits per version (v1-10)
    private static readonly int[] REMAINDER = { 0, 0, 7, 7, 7, 7, 7, 0, 0, 0, 0 };

    // EC level indicator bits: L=01, M=00, Q=11, H=10
    private static readonly Dictionary<string, int> EcIndicator = new()
    {
        ["L"] = 1, ["M"] = 0, ["Q"] = 3, ["H"] = 2,
    };

    // ═══════════════════════════════════════════════
    //  Data encoding (byte mode)
    // ═══════════════════════════════════════════════

    private static int[] TextToBytes(string text)
    {
        // Match JS: UTF-8 encoding over UTF-16 code units (surrogate pairs -> 4 bytes).
        // .NET's UTF8 encoder produces the same byte sequence.
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = new int[bytes.Length];
        for (var i = 0; i < bytes.Length; i++) result[i] = bytes[i];
        return result;
    }

    private static int GetDataCapacity(int version, string ecLevel)
    {
        var ec = EC[version]![ecLevel];
        var total = 0;
        foreach (var (count, dcw) in ec.Groups) total += count * dcw;
        return total;
    }

    private static int GetMinVersion(int dataLen, string ecLevel)
    {
        for (var v = 1; v <= 10; v++)
        {
            var capacity = GetDataCapacity(v, ecLevel);
            var countBits = v <= 9 ? 8 : 16;
            var maxChars = (capacity * 8 - 4 - countBits) / 8;
            if (dataLen <= maxChars) return v;
        }
        return -1;
    }

    private static int[] CreateDataBits(int[] data, int version, string ecLevel)
    {
        var capacity = GetDataCapacity(version, ecLevel);
        var countBits = version <= 9 ? 8 : 16;
        var bits = new List<int>();

        // Mode indicator: byte mode = 0100
        bits.Add(0); bits.Add(1); bits.Add(0); bits.Add(0);

        // Character count
        for (var i = countBits - 1; i >= 0; i--)
            bits.Add((data.Length >> i) & 1);

        // Data bytes
        for (var i = 0; i < data.Length; i++)
            for (var b = 7; b >= 0; b--)
                bits.Add((data[i] >> b) & 1);

        // Terminator (up to 4 zero bits)
        var totalBits = capacity * 8;
        var termLen = Math.Min(4, totalBits - bits.Count);
        for (var i = 0; i < termLen; i++) bits.Add(0);

        // Pad to byte boundary
        while (bits.Count % 8 != 0) bits.Add(0);

        // Pad codewords (236, 17 alternating)
        int[] padBytes = { 236, 17 };
        var padIdx = 0;
        while (bits.Count < totalBits)
        {
            var pb = padBytes[padIdx % 2];
            for (var b = 7; b >= 0; b--) bits.Add((pb >> b) & 1);
            padIdx++;
        }

        // Convert bits to codewords
        var codewords = new List<int>();
        for (var i = 0; i < bits.Count; i += 8)
        {
            var val = 0;
            for (var b = 0; b < 8; b++) val = (val << 1) | bits[i + b];
            codewords.Add(val);
        }
        return codewords.ToArray();
    }

    // ═══════════════════════════════════════════════
    //  Block splitting, EC generation, interleaving
    // ═══════════════════════════════════════════════

    private sealed class Block
    {
        public int[] Data = Array.Empty<int>();
        public int[] Ecc = Array.Empty<int>();
    }

    private static (List<Block> Blocks, int EccPerBlock) CreateBlocks(int[] dataCW, int version, string ecLevel)
    {
        var ec = EC[version]![ecLevel];
        var eccPerBlock = ec.EccPerBlock;
        var blocks = new List<Block>();
        var offset = 0;

        foreach (var (count, dcw) in ec.Groups)
        {
            for (var b = 0; b < count; b++)
            {
                var blockData = dataCW.Skip(offset).Take(dcw).ToArray();
                var blockEcc = RsEncode(blockData, eccPerBlock);
                blocks.Add(new Block { Data = blockData, Ecc = blockEcc });
                offset += dcw;
            }
        }
        return (blocks, eccPerBlock);
    }

    private static int[] Interleave(List<Block> blocks, int eccPerBlock)
    {
        var result = new List<int>();
        var maxDataLen = 0;
        foreach (var blk in blocks)
            if (blk.Data.Length > maxDataLen) maxDataLen = blk.Data.Length;

        // Interleave data codewords
        for (var i = 0; i < maxDataLen; i++)
            foreach (var blk in blocks)
                if (i < blk.Data.Length) result.Add(blk.Data[i]);

        // Interleave EC codewords
        for (var i = 0; i < eccPerBlock; i++)
            foreach (var blk in blocks)
                result.Add(blk.Ecc[i]);

        return result.ToArray();
    }

    private static int[] CodewordsToBits(int[] codewords, int version)
    {
        var bits = new List<int>();
        foreach (var cw in codewords)
            for (var b = 7; b >= 0; b--)
                bits.Add((cw >> b) & 1);

        // Remainder bits
        var rem = REMAINDER[version];
        for (var i = 0; i < rem; i++) bits.Add(0);

        return bits.ToArray();
    }

    // ═══════════════════════════════════════════════
    //  Matrix construction
    // ═══════════════════════════════════════════════

    private sealed class Matrix
    {
        public bool[][] Modules;
        public bool[][] IsFunc;
        public int Size;

        public Matrix(int size)
        {
            Size = size;
            Modules = new bool[size][];
            IsFunc = new bool[size][];
            for (var r = 0; r < size; r++)
            {
                Modules[r] = new bool[size];
                IsFunc[r] = new bool[size];
            }
        }
    }

    private static void SetModule(Matrix m, int row, int col, bool dark, bool isFunction)
    {
        if (row >= 0 && row < m.Size && col >= 0 && col < m.Size)
        {
            m.Modules[row][col] = dark;
            if (isFunction) m.IsFunc[row][col] = true;
        }
    }

    private static void PlaceFinderPattern(Matrix m, int row, int col)
    {
        for (var dr = -1; dr <= 7; dr++)
        {
            for (var dc = -1; dc <= 7; dc++)
            {
                var r = row + dr;
                var c = col + dc;
                if (r < 0 || r >= m.Size || c < 0 || c >= m.Size) continue;
                var dark =
                    (dr >= 0 && dr <= 6 && (dc == 0 || dc == 6)) ||
                    (dc >= 0 && dc <= 6 && (dr == 0 || dr == 6)) ||
                    (dr >= 2 && dr <= 4 && dc >= 2 && dc <= 4);
                SetModule(m, r, c, dark, true);
            }
        }
    }

    private static void PlaceAlignmentPattern(Matrix m, int row, int col)
    {
        for (var dr = -2; dr <= 2; dr++)
        {
            for (var dc = -2; dc <= 2; dc++)
            {
                var dark = Math.Abs(dr) == 2 || Math.Abs(dc) == 2 || (dr == 0 && dc == 0);
                SetModule(m, row + dr, col + dc, dark, true);
            }
        }
    }

    private static void PlaceFunctionPatterns(Matrix m, int version)
    {
        var size = m.Size;

        // Finder patterns + separators
        PlaceFinderPattern(m, 0, 0);
        PlaceFinderPattern(m, 0, size - 7);
        PlaceFinderPattern(m, size - 7, 0);

        // Timing patterns
        for (var i = 8; i < size - 8; i++)
        {
            SetModule(m, 6, i, i % 2 == 0, true);
            SetModule(m, i, 6, i % 2 == 0, true);
        }

        // Alignment patterns
        var positions = ALIGN[version];
        if (positions.Length > 0)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                for (var j = 0; j < positions.Length; j++)
                {
                    var r = positions[i];
                    var c = positions[j];
                    if (r <= 8 && c <= 8) continue;
                    if (r <= 8 && c >= size - 8) continue;
                    if (r >= size - 8 && c <= 8) continue;
                    PlaceAlignmentPattern(m, r, c);
                }
            }
        }

        // Dark module
        SetModule(m, 4 * version + 9, 8, true, true);

        // Reserve format info areas
        for (var i = 0; i < 8; i++)
        {
            SetModule(m, 8, i, false, true);
            SetModule(m, 8, size - 1 - i, false, true);
            SetModule(m, i, 8, false, true);
            SetModule(m, size - 1 - i, 8, false, true);
        }
        SetModule(m, 8, 8, false, true);

        // Reserve version info areas (v >= 7)
        if (version >= 7)
        {
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    SetModule(m, i, size - 11 + j, false, true);
                    SetModule(m, size - 11 + j, i, false, true);
                }
            }
        }
    }

    // ═══════════════════════════════════════════════
    //  Data placement (zigzag)
    // ═══════════════════════════════════════════════

    private static void PlaceData(Matrix m, int[] bits)
    {
        var size = m.Size;
        var bitIdx = 0;
        var upward = true;

        for (var right = size - 1; right >= 1; right -= 2)
        {
            if (right == 6) right = 5; // skip timing column

            for (var vert = 0; vert < size; vert++)
            {
                var row = upward ? (size - 1 - vert) : vert;

                for (var dx = 0; dx <= 1; dx++)
                {
                    var col = right - dx;
                    if (col < 0) continue;
                    if (m.IsFunc[row][col]) continue;

                    m.Modules[row][col] = bitIdx < bits.Length && bits[bitIdx] == 1;
                    bitIdx++;
                }
            }

            upward = !upward;
        }
    }

    // ═══════════════════════════════════════════════
    //  Masking
    // ═══════════════════════════════════════════════

    private static readonly Func<int, int, bool>[] MaskFns =
    {
        (r, c) => (r + c) % 2 == 0,
        (r, c) => r % 2 == 0,
        (r, c) => c % 3 == 0,
        (r, c) => (r + c) % 3 == 0,
        (r, c) => ((r / 2) + (c / 3)) % 2 == 0,
        (r, c) => (r * c) % 2 + (r * c) % 3 == 0,
        (r, c) => ((r * c) % 2 + (r * c) % 3) % 2 == 0,
        (r, c) => ((r + c) % 2 + (r * c) % 3) % 2 == 0,
    };

    private static void ApplyMask(Matrix m, int maskIdx)
    {
        var fn = MaskFns[maskIdx];
        for (var r = 0; r < m.Size; r++)
            for (var c = 0; c < m.Size; c++)
                if (!m.IsFunc[r][c] && fn(r, c))
                    m.Modules[r][c] = !m.Modules[r][c];
    }

    private static int CalcPenalty(Matrix m)
    {
        var size = m.Size;
        var penalty = 0;
        var mod = m.Modules;

        // Rule 1: Runs of same color (rows and columns)
        for (var r = 0; r < size; r++)
        {
            var runLen = 1;
            for (var c = 1; c < size; c++)
            {
                if (mod[r][c] == mod[r][c - 1]) runLen++;
                else
                {
                    if (runLen >= 5) penalty += runLen - 2;
                    runLen = 1;
                }
            }
            if (runLen >= 5) penalty += runLen - 2;
        }
        for (var c = 0; c < size; c++)
        {
            var runLen = 1;
            for (var r = 1; r < size; r++)
            {
                if (mod[r][c] == mod[r - 1][c]) runLen++;
                else
                {
                    if (runLen >= 5) penalty += runLen - 2;
                    runLen = 1;
                }
            }
            if (runLen >= 5) penalty += runLen - 2;
        }

        // Rule 2: 2x2 blocks of same color
        for (var r = 0; r < size - 1; r++)
            for (var c = 0; c < size - 1; c++)
                if (mod[r][c] == mod[r][c + 1] &&
                    mod[r][c] == mod[r + 1][c] &&
                    mod[r][c] == mod[r + 1][c + 1])
                    penalty += 3;

        // Rule 3: Finder-like patterns
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c <= size - 11; c++)
            {
                if (mod[r][c] && !mod[r][c + 1] && mod[r][c + 2] && mod[r][c + 3] &&
                    mod[r][c + 4] && !mod[r][c + 5] && mod[r][c + 6] &&
                    !mod[r][c + 7] && !mod[r][c + 8] && !mod[r][c + 9] && !mod[r][c + 10])
                    penalty += 40;
                if (!mod[r][c] && !mod[r][c + 1] && !mod[r][c + 2] && !mod[r][c + 3] &&
                    mod[r][c + 4] && !mod[r][c + 5] && mod[r][c + 6] && mod[r][c + 7] &&
                    mod[r][c + 8] && !mod[r][c + 9] && mod[r][c + 10])
                    penalty += 40;
            }
        }
        for (var c = 0; c < size; c++)
        {
            for (var r = 0; r <= size - 11; r++)
            {
                if (mod[r][c] && !mod[r + 1][c] && mod[r + 2][c] && mod[r + 3][c] &&
                    mod[r + 4][c] && !mod[r + 5][c] && mod[r + 6][c] &&
                    !mod[r + 7][c] && !mod[r + 8][c] && !mod[r + 9][c] && !mod[r + 10][c])
                    penalty += 40;
                if (!mod[r][c] && !mod[r + 1][c] && !mod[r + 2][c] && !mod[r + 3][c] &&
                    mod[r + 4][c] && !mod[r + 5][c] && mod[r + 6][c] && mod[r + 7][c] &&
                    mod[r + 8][c] && !mod[r + 9][c] && mod[r + 10][c])
                    penalty += 40;
            }
        }

        // Rule 4: Dark module proportion
        var darkCount = 0;
        for (var r = 0; r < size; r++)
            for (var c = 0; c < size; c++)
                if (mod[r][c]) darkCount++;

        var ratio = darkCount * 100.0 / (size * size);
        penalty += (int)Math.Floor(Math.Abs(ratio - 50) / 5) * 10;

        return penalty;
    }

    // ═══════════════════════════════════════════════
    //  Format & version information
    // ═══════════════════════════════════════════════

    private static int EncodeFormatInfo(string ecLevel, int maskIdx)
    {
        var data = (EcIndicator[ecLevel] << 3) | maskIdx;
        var rem = data;
        for (var i = 0; i < 10; i++)
            rem = (rem << 1) ^ ((rem >> 9) != 0 ? 0x537 : 0);
        var bits = ((data << 10) | rem) ^ 0x5412;
        return bits;
    }

    private static void PlaceFormatInfo(Matrix m, int formatBits)
    {
        var size = m.Size;

        // Around top-left finder
        for (var i = 0; i < 6; i++)
            m.Modules[8][i] = ((formatBits >> (14 - i)) & 1) == 1;
        m.Modules[8][7] = ((formatBits >> 8) & 1) == 1;
        m.Modules[8][8] = ((formatBits >> 7) & 1) == 1;
        m.Modules[7][8] = ((formatBits >> 6) & 1) == 1;
        for (var i = 0; i < 6; i++)
            m.Modules[5 - i][8] = ((formatBits >> (5 - i)) & 1) == 1;

        // Around top-right and bottom-left finders
        for (var i = 0; i < 8; i++)
            m.Modules[8][size - 1 - i] = ((formatBits >> i) & 1) == 1;
        for (var i = 0; i < 7; i++)
            m.Modules[size - 1 - i][8] = ((formatBits >> (14 - i)) & 1) == 1;
    }

    private static int EncodeVersionInfo(int version)
    {
        var rem = version;
        for (var i = 0; i < 12; i++)
            rem = (rem << 1) ^ ((rem >> 11) != 0 ? 0x1f25 : 0);
        return (version << 12) | rem;
    }

    private static void PlaceVersionInfo(Matrix m, int version)
    {
        if (version < 7) return;
        var versionBits = EncodeVersionInfo(version);
        var size = m.Size;

        for (var i = 0; i < 18; i++)
        {
            var bit = ((versionBits >> i) & 1) == 1;
            var row = i / 3;
            var col = size - 11 + (i % 3);
            m.Modules[row][col] = bit;
            m.Modules[col][row] = bit;
        }
    }

    // ═══════════════════════════════════════════════
    //  Main QR generation
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Generates the QR module matrix for the given text and error-correction level.
    /// Returns <c>null</c> when the text is empty or does not fit within versions 1–10.
    /// </summary>
    /// <param name="text">The data to encode (UTF-8, byte mode).</param>
    /// <param name="ecLevel">Error-correction level: "L", "M", "Q", or "H". Defaults to "M".</param>
    /// <returns>
    /// A jagged <c>bool[][]</c> indexed <c>[row][col]</c> where <c>true</c> means a dark module,
    /// or <c>null</c> when the input cannot be encoded.
    /// </returns>
    public static bool[][]? Generate(string? text, string? ecLevel = "M")
    {
        if (string.IsNullOrEmpty(text)) return null;
        var level = string.IsNullOrEmpty(ecLevel) ? "M" : ecLevel!;
        if (!EcIndicator.ContainsKey(level)) level = "M";

        var data = TextToBytes(text);
        var version = GetMinVersion(data.Length, level);
        if (version < 0) return null;

        var dataCW = CreateDataBits(data, version, level);
        var (blocks, eccPerBlock) = CreateBlocks(dataCW, version, level);
        var interleavedCW = Interleave(blocks, eccPerBlock);
        var bits = CodewordsToBits(interleavedCW, version);

        var size = version * 4 + 17;
        var m = new Matrix(size);
        PlaceFunctionPatterns(m, version);
        PlaceData(m, bits);

        // Try all 8 masks, choose lowest penalty
        var bestMask = 0;
        var bestPenalty = int.MaxValue;

        for (var mask = 0; mask < 8; mask++)
        {
            // Clone modules for testing
            var testMod = new bool[size][];
            for (var r = 0; r < size; r++) testMod[r] = (bool[])m.Modules[r].Clone();

            ApplyMask(m, mask);
            var fb = EncodeFormatInfo(level, mask);
            PlaceFormatInfo(m, fb);
            PlaceVersionInfo(m, version);

            var penalty = CalcPenalty(m);
            if (penalty < bestPenalty)
            {
                bestPenalty = penalty;
                bestMask = mask;
            }

            // Restore modules
            for (var r = 0; r < size; r++) m.Modules[r] = testMod[r];
        }

        // Apply best mask
        ApplyMask(m, bestMask);
        var formatBits = EncodeFormatInfo(level, bestMask);
        PlaceFormatInfo(m, formatBits);
        PlaceVersionInfo(m, version);

        return m.Modules;
    }
}
