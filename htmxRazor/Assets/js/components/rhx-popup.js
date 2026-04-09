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

      // Use CSS Anchor Positioning when supported
      if (useCssAnchoring && !cssAnchoringApplied) {
        window.RHX.applyCssAnchorPositioning(anchor, popup, {
          placement: placement,
          distance: distance
        });
        cssAnchoringApplied = true;
        return;
      }

      if (useCssAnchoring) return; // CSS handles repositioning automatically

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
