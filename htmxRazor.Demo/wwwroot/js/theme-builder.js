/**
 * htmxRazor Theme Builder
 * Applies CSS custom property changes in real-time to the preview panel.
 */
(function () {
  "use strict";

  var preview = document.getElementById("theme-preview");
  var downloadForm = document.getElementById("download-form");
  var changeCountEl = document.getElementById("change-count");

  /**
   * Apply a single token value to the preview container.
   */
  function applyToken(input) {
    var prop = input.getAttribute("data-rhx-token");
    var value = input.value;
    if (prop && preview) {
      preview.style.setProperty(prop, value);
    }

    // Sync hex text input with color picker
    var hexInput = document.querySelector('[data-rhx-hex-for="' + prop + '"]');
    if (hexInput && hexInput !== input) {
      hexInput.value = value;
    }

    syncDownloadForm();
  }

  /**
   * Apply a hex value typed into the text input companion of a color picker.
   */
  function applyHexToken(input) {
    var prop = input.getAttribute("data-rhx-hex-for");
    var value = input.value;
    if (prop && preview) {
      preview.style.setProperty(prop, value);
    }

    // Sync color picker
    var colorInput = document.querySelector('[data-rhx-token="' + prop + '"]');
    if (colorInput && colorInput !== input && /^#[0-9a-fA-F]{6}$/.test(value)) {
      colorInput.value = value;
    }

    syncDownloadForm();
  }

  /**
   * Sync all changed tokens into the download form as hidden inputs
   * and update the change counter badge.
   */
  function syncDownloadForm() {
    if (!downloadForm) return;

    // Remove only our token hidden inputs (preserve antiforgery token and other framework inputs)
    downloadForm.querySelectorAll("input[data-rhx-generated]").forEach(function (el) {
      el.remove();
    });

    // Add hidden inputs for changed values
    var count = 0;
    document.querySelectorAll("[data-rhx-token]").forEach(function (input) {
      var prop = input.getAttribute("data-rhx-token");
      var defaultVal = input.getAttribute("data-rhx-default");
      var current = input.value;

      if (current !== defaultVal) {
        var hidden = document.createElement("input");
        hidden.type = "hidden";
        hidden.name = prop;
        hidden.value = current;
        hidden.setAttribute("data-rhx-generated", "");
        downloadForm.appendChild(hidden);
        count++;
      }
    });

    // Update change count badge
    if (changeCountEl) {
      changeCountEl.textContent = count > 0 ? "(" + count + ")" : "";
    }
  }

  /**
   * Reset all tokens to their default values.
   */
  function resetTokens() {
    document.querySelectorAll("[data-rhx-token]").forEach(function (input) {
      var defaultVal = input.getAttribute("data-rhx-default");
      if (defaultVal) {
        input.value = defaultVal;
        var prop = input.getAttribute("data-rhx-token");
        if (preview) {
          preview.style.removeProperty(prop);
        }

        // Sync hex input
        var hexInput = document.querySelector('[data-rhx-hex-for="' + prop + '"]');
        if (hexInput) {
          hexInput.value = defaultVal;
        }
      }
    });
    syncDownloadForm();
  }

  /**
   * Auto-generate a full palette from the current 500 shade value.
   * Called by the "Generate palette from 500 shade" button.
   *
   * @param {string} paletteName - e.g. "brand", "success", "danger"
   */
  function autoGeneratePalette(paletteName) {
    var baseInput = document.querySelector('[data-rhx-token="--rhx-color-' + paletteName + '-500"]');
    if (!baseInput) return;

    var palette = generatePalette(baseInput.value);
    if (!palette) return;

    var steps = ["50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950"];
    steps.forEach(function (step) {
      var hex = palette[step];
      if (!hex) return;

      var prop = "--rhx-color-" + paletteName + "-" + step;
      var colorInput = document.querySelector('[data-rhx-token="' + prop + '"]');
      if (colorInput) {
        colorInput.value = hex;
        if (preview) preview.style.setProperty(prop, hex);
      }

      var hexInput = document.querySelector('[data-rhx-hex-for="' + prop + '"]');
      if (hexInput) {
        hexInput.value = hex;
      }
    });

    syncDownloadForm();
  }

  /**
   * Generate a full color palette from a single base color (the 500 shade).
   * Uses HSL manipulation to create lighter (50-400) and darker (600-950) shades.
   */
  function generatePalette(hex) {
    var rgb = hexToRgb(hex);
    if (!rgb) return null;

    var hsl = rgbToHsl(rgb.r, rgb.g, rgb.b);

    // Shade lightness targets (approximate Tailwind-like distribution)
    var targets = {
      "50":  { l: 0.97, s: hsl.s * 0.3 },
      "100": { l: 0.93, s: hsl.s * 0.4 },
      "200": { l: 0.87, s: hsl.s * 0.5 },
      "300": { l: 0.76, s: hsl.s * 0.6 },
      "400": { l: 0.65, s: hsl.s * 0.8 },
      "500": { l: hsl.l, s: hsl.s },
      "600": { l: hsl.l * 0.82, s: hsl.s * 1.05 },
      "700": { l: hsl.l * 0.65, s: hsl.s * 1.1 },
      "800": { l: hsl.l * 0.50, s: hsl.s * 1.05 },
      "900": { l: hsl.l * 0.38, s: hsl.s * 1.0 },
      "950": { l: hsl.l * 0.22, s: hsl.s * 0.9 }
    };

    var result = {};
    for (var step in targets) {
      var t = targets[step];
      var adjustedRgb = hslToRgb(hsl.h, Math.min(t.s, 1), Math.min(t.l, 1));
      result[step] = rgbToHex(adjustedRgb.r, adjustedRgb.g, adjustedRgb.b);
    }
    return result;
  }

  // ── Color utility functions ──

  function hexToRgb(hex) {
    hex = hex.replace(/^#/, "");
    if (hex.length !== 6) return null;
    return {
      r: parseInt(hex.substr(0, 2), 16),
      g: parseInt(hex.substr(2, 2), 16),
      b: parseInt(hex.substr(4, 2), 16)
    };
  }

  function rgbToHex(r, g, b) {
    return "#" + [r, g, b].map(function (x) {
      var h = Math.round(Math.max(0, Math.min(255, x))).toString(16);
      return h.length === 1 ? "0" + h : h;
    }).join("");
  }

  function rgbToHsl(r, g, b) {
    r /= 255; g /= 255; b /= 255;
    var max = Math.max(r, g, b), min = Math.min(r, g, b);
    var h, s, l = (max + min) / 2;

    if (max === min) {
      h = s = 0;
    } else {
      var d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
        case g: h = ((b - r) / d + 2) / 6; break;
        case b: h = ((r - g) / d + 4) / 6; break;
      }
    }
    return { h: h, s: s, l: l };
  }

  function hslToRgb(h, s, l) {
    var r, g, b;
    if (s === 0) {
      r = g = b = l;
    } else {
      function hue2rgb(p, q, t) {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1/6) return p + (q - p) * 6 * t;
        if (t < 1/2) return q;
        if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
        return p;
      }
      var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
      var p = 2 * l - q;
      r = hue2rgb(p, q, h + 1/3);
      g = hue2rgb(p, q, h);
      b = hue2rgb(p, q, h - 1/3);
    }
    return { r: r * 255, g: g * 255, b: b * 255 };
  }

  // Expose to global scope for inline event handlers
  window.applyToken = applyToken;
  window.applyHexToken = applyHexToken;
  window.resetTokens = resetTokens;
  window.autoGeneratePalette = autoGeneratePalette;
  window.generatePalette = generatePalette;
})();
