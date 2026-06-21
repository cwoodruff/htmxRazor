/**
 * htmxRazor Popup Component
 * Low-level positioning utility using the shared rhx-position.js engine.
 */
(function () {
  "use strict";

  function initPopup(popup) {
    if (popup._rhxPopupInit) return;
    popup._rhxPopupInit = true;

    var anchorSel = popup.getAttribute("data-rhx-anchor");
    if (!anchorSel) return;

    var anchor = document.querySelector(anchorSel);
    if (!anchor) return;

    var arrowEl = popup.querySelector("[data-rhx-popup-arrow]");
    var useCssAnchoring = window.RHX && window.RHX.supportsAnchorPositioning;
    var cssAnchoringApplied = false;

    function reposition() {
      if (popup.hidden) return;

      var placement = popup.getAttribute("data-rhx-placement") || "bottom-start";
      var distance = parseInt(popup.getAttribute("data-rhx-distance") || "4", 10);

      // Use CSS Anchor Positioning when supported (apply once, CSS handles the rest)
      if (useCssAnchoring && !cssAnchoringApplied) {
        window.RHX.applyCssAnchorPositioning(anchor, popup, {
          placement: placement,
          distance: distance
        });
        cssAnchoringApplied = true;
      }

      // In CSS anchor mode, position the arrow after the browser lays out the popup
      if (useCssAnchoring) {
        popup.setAttribute("data-rhx-current-placement", placement);
        if (arrowEl) {
          // Use requestAnimationFrame to read geometry after CSS positions the popup
          requestAnimationFrame(function () {
            var anchorRect = anchor.getBoundingClientRect();
            var popupRect = popup.getBoundingClientRect();
            var side = placement.split("-")[0];
            if (side === "top" || side === "bottom") {
              // Position arrow horizontally to align with anchor center
              var cx = anchorRect.left + anchorRect.width / 2 - popupRect.left - 5;
              arrowEl.style.left = Math.max(8, Math.min(cx, popupRect.width - 18)) + "px";
              // Position arrow vertically outside the popup
              if (side === "top") {
                // Popup above anchor, arrow at bottom pointing down
                arrowEl.style.top = (popupRect.height - 5) + "px";
              } else {
                // Popup below anchor, arrow at top pointing up
                arrowEl.style.top = "-5px";
              }
            } else {
              // Position arrow vertically to align with anchor center
              var cy = anchorRect.top + anchorRect.height / 2 - popupRect.top - 5;
              arrowEl.style.top = Math.max(8, Math.min(cy, popupRect.height - 18)) + "px";
              // Position arrow horizontally outside the popup
              if (side === "left") {
                // Popup left of anchor, arrow at right pointing right
                arrowEl.style.left = (popupRect.width - 5) + "px";
              } else {
                // Popup right of anchor, arrow at left pointing left
                arrowEl.style.left = "-5px";
              }
            }
          });
        }
        return; // CSS handles repositioning automatically
      }

      // Fallback: JS positioning
      if (!window.RHX || !window.RHX.positionElement) return;

      window.RHX.positionElement(anchor, popup, {
        placement: placement,
        distance: distance,
        skidding: parseInt(popup.getAttribute("data-rhx-skidding") || "0", 10),
        strategy: popup.getAttribute("data-rhx-strategy") || "absolute",
        flip: !popup.hasAttribute("data-rhx-no-flip"),
        shift: !popup.hasAttribute("data-rhx-no-shift"),
        arrowElement: arrowEl,
        arrowPadding: parseInt(popup.getAttribute("data-rhx-arrow-padding") || "8", 10)
      });
    }

    // Position if already active
    if (popup.classList.contains("rhx-popup--active")) {
      reposition();
    }

    // Observe attribute changes to reposition on active toggle
    var observer = new MutationObserver(function () {
      if (popup.classList.contains("rhx-popup--active") && !popup.hidden) {
        reposition();
      }
    });
    observer.observe(popup, { attributes: true, attributeFilter: ["class", "hidden"] });

    // Reposition on scroll/resize
    window.addEventListener("scroll", reposition, { passive: true });
    window.addEventListener("resize", reposition, { passive: true });

    // Expose reposition for programmatic use
    popup._rhxReposition = reposition;
  }

  function initPopups(root) {
    if (root && root.querySelectorAll) {
      root.querySelectorAll("[data-rhx-popup]").forEach(initPopup);
    }
    if (root && root.matches && root.matches("[data-rhx-popup]")) {
      initPopup(root);
    }
  }

  // Register with RHX core
  if (typeof RHX !== "undefined" && RHX.register) {
    RHX.register("popup", initPopups);
  }

  // Self-init fallback
  function initAll() {
    document.querySelectorAll("[data-rhx-popup]").forEach(initPopup);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initAll);
  } else {
    initAll();
  }

  // Re-init on htmx content swap
  document.addEventListener("htmx:afterSettle", function (e) {
    var el = e.detail.elt;
    if (el && el.querySelectorAll) {
      el.querySelectorAll("[data-rhx-popup]").forEach(initPopup);
    }
    if (el && el.matches && el.matches("[data-rhx-popup]")) {
      initPopup(el);
    }
  });
})();
