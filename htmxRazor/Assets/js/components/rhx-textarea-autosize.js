/**
 * rhx-textarea-autosize.js — SANCTIONED *TEMPORARY* JS HOLDOUT (JS* #9)
 *
 * Why this exists: `field-sizing: content` grows a <textarea> to fit its content
 * with zero JS, and rhx-textarea ships that CSS now — but Firefox does not yet
 * support `field-sizing` (spike 2026-06-14). This script is the fallback for
 * browsers without it. It is explicitly TEMPORARY: once `field-sizing` is
 * cross-browser, the CSS fully covers autosize and this file is deleted.
 *
 * Feature-detected: if the browser supports field-sizing, the script no-ops and
 * the CSS does the work. Otherwise it sets height to scrollHeight on input.
 */
(function () {
  "use strict";

  // If the platform supports field-sizing, the CSS handles autosize — do nothing.
  var supportsFieldSizing =
    window.CSS && CSS.supports && CSS.supports("field-sizing", "content");
  if (supportsFieldSizing) return;

  function resize(textarea) {
    textarea.style.height = "auto";
    textarea.style.height = textarea.scrollHeight + "px";
  }

  function init(root) {
    (root || document).querySelectorAll("[data-rhx-autosize]").forEach(function (ta) {
      if (ta._rhxAutosize) return;
      ta._rhxAutosize = true;
      ta.addEventListener("input", function () { resize(ta); });
      resize(ta);
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", function () { init(document); });
  } else {
    init(document);
  }
  document.addEventListener("htmx:afterSwap", function (e) { init(e.detail.target); });
})();
