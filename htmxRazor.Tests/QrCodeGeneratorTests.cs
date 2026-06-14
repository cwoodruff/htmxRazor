using htmxRazor.Components.Utilities;
using Xunit;

namespace htmxRazor.Tests;

/// <summary>
/// Verifies the server-side <see cref="QrCodeGenerator"/> produces the exact same
/// module matrix the previous client-side JavaScript encoder produced. The golden
/// fixtures below were dumped from the original <c>rhx-qr-code.js</c> via Node and
/// are reproduced here as a byte-for-byte equivalence guard.
/// </summary>
public class QrCodeGeneratorTests
{
    private static string Dump(bool[][] m) =>
        string.Join("\n", m.Select(row => string.Concat(row.Select(b => b ? '1' : '0'))));

    // ── Equivalence against JS-produced golden matrices ──

    [Fact]
    public void Matches_Js_For_Numeric_M()
    {
        const string expected =
            "111111101011101111111\n" +
            "100000100110101000001\n" +
            "101110100110001011101\n" +
            "101110101010101011101\n" +
            "101110101100101011101\n" +
            "100000101101001000001\n" +
            "111111100010101111111\n" +
            "000000001011100000000\n" +
            "100010011101011111001\n" +
            "111000001001100100100\n" +
            "001010111011001111100\n" +
            "001011000010011011010\n" +
            "110111100000111001011\n" +
            "000000000000111000100\n" +
            "111111101010110000100\n" +
            "100000100001100101010\n" +
            "101110101011001111001\n" +
            "101110100111100001111\n" +
            "101110100111001111000\n" +
            "100000100110011011000\n" +
            "111111101010111101001";

        var m = QrCodeGenerator.Generate("12345", "M");
        Assert.NotNull(m);
        Assert.Equal(expected, Dump(m!));
    }

    [Fact]
    public void Matches_Js_For_Hello_World_M()
    {
        const string expected =
            "111111101100101111111\n" +
            "100000100001001000001\n" +
            "101110100101001011101\n" +
            "101110101001001011101\n" +
            "101110101110101011101\n" +
            "100000101001001000001\n" +
            "111111100010101111111\n" +
            "000000001001100000000\n" +
            "100010011111011111001\n" +
            "000100001011100001111\n" +
            "001111110011011010010\n" +
            "111110001100010000000\n" +
            "111110101010101100110\n" +
            "000000000010111101011\n" +
            "111111101110101011010\n" +
            "100000100101110110011\n" +
            "101110101101011000110\n" +
            "101110100100100011011\n" +
            "101110100111000111000\n" +
            "100000100001010000000\n" +
            "111111101111111110101";

        var m = QrCodeGenerator.Generate("HELLO WORLD", "M");
        Assert.NotNull(m);
        Assert.Equal(expected, Dump(m!));
    }

    // ── Structural properties (version selection / dimensions) ──

    [Theory]
    [InlineData("https://example.com", "L", 25)]
    [InlineData("https://example.com", "M", 25)]
    [InlineData("https://example.com", "Q", 25)]
    [InlineData("https://example.com", "H", 29)]
    [InlineData("HELLO WORLD", "L", 21)]
    [InlineData("HELLO WORLD", "H", 25)]
    [InlineData("12345", "H", 21)]
    public void Produces_Expected_Dimensions(string text, string ec, int expectedSize)
    {
        var m = QrCodeGenerator.Generate(text, ec);
        Assert.NotNull(m);
        Assert.Equal(expectedSize, m!.Length);
        Assert.All(m, row => Assert.Equal(expectedSize, row.Length));
    }

    [Fact]
    public void Long_Input_Selects_Higher_Versions_Per_Level()
    {
        const string longText =
            "The quick brown fox jumps over the lazy dog. " +
            "Pack my box with five dozen liquor jugs! 0123456789";

        Assert.Equal(37, QrCodeGenerator.Generate(longText, "L")!.Length); // v5
        Assert.Equal(41, QrCodeGenerator.Generate(longText, "M")!.Length); // v6
        Assert.Equal(49, QrCodeGenerator.Generate(longText, "Q")!.Length); // v8
        Assert.Equal(53, QrCodeGenerator.Generate(longText, "H")!.Length); // v9
    }

    [Fact]
    public void Finder_Patterns_Are_Present()
    {
        var m = QrCodeGenerator.Generate("HELLO WORLD", "M")!;
        var n = m.Length;

        // Top-left, top-right and bottom-left finder corners are dark.
        Assert.True(m[0][0]);
        Assert.True(m[0][n - 1]);
        Assert.True(m[n - 1][0]);
        // Centre of the top-left finder is dark, the surrounding ring is light.
        Assert.True(m[3][3]);
        Assert.False(m[5][5]);
    }

    // ── Edge cases ──

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Empty_Input_Returns_Null(string? text)
    {
        Assert.Null(QrCodeGenerator.Generate(text, "M"));
    }

    [Fact]
    public void Invalid_Or_Missing_Level_Defaults_To_M()
    {
        var def = QrCodeGenerator.Generate("HELLO WORLD", null);
        var m = QrCodeGenerator.Generate("HELLO WORLD", "M");
        var bogus = QrCodeGenerator.Generate("HELLO WORLD", "Z");

        Assert.NotNull(def);
        Assert.Equal(Dump(m!), Dump(def!));
        Assert.Equal(Dump(m!), Dump(bogus!));
    }
}
