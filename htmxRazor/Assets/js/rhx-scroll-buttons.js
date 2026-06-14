/**
 * rhx-scroll-buttons.js — TEMPORARY, SANCTIONED holdout (migration plan JS* #10).
 *
 * Scroll-snap scrolling/swiping/keyboard works with zero JS. Only the prev/next BUTTONS
 * need a script today because the declarative `::scroll-button()` pseudo-element is
 * Chromium-only (spike 2026-06-14). This ~30-line shim is the single allowlisted file for
 * carousel + scroller scroll buttons; DELETE IT once ::scroll-button() ships cross-browser.
 *
 * Contract (emitted by the Tag Helpers, no per-component JS):
 *   <div data-rhx-scrollable>
 *     <button data-rhx-scroll-prev>…</button>
 *     <div data-rhx-scroll-viewport> … snap items … </div>
 *     <button data-rhx-scroll-next>…</button>
 *   </div>
 * Clicking prev/next scrolls the nearest [data-rhx-scroll-viewport] by ~90% of its width
 * (or height, when it scrolls vertically).
 */
(function () {
  "use strict";

  function viewportFor(button) {
    var scope = button.closest("[data-rhx-scrollable]");
    return scope ? scope.querySelector("[data-rhx-scroll-viewport]") : null;
  }

  function scroll(button, dir) {
    var vp = viewportFor(button);
    if (!vp) return;
    var vertical = vp.scrollHeight > vp.clientHeight && vp.scrollWidth <= vp.clientWidth;
    if (vertical) {
      vp.scrollBy({ top: dir * vp.clientHeight * 0.9, behavior: "smooth" });
    } else {
      vp.scrollBy({ left: dir * vp.clientWidth * 0.9, behavior: "smooth" });
    }
  }

  document.addEventListener("click", function (e) {
    var prev = e.target.closest("[data-rhx-scroll-prev]");
    if (prev) { scroll(prev, -1); return; }
    var next = e.target.closest("[data-rhx-scroll-next]");
    if (next) { scroll(next, 1); }
  });
})();
