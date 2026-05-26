/**
 * htmxRazor Popover Component
 * Content-rich popover with click/hover/focus triggers.
 * Uses the shared rhx-position.js engine for positioning.
 */
(function () {
  "use strict";

  function initPopovers(root) {
    var popovers = root.querySelectorAll("[data-rhx-popover]");
    popovers.forEach(function (popover) {
      if (popover._rhxPopoverInit) return;
      popover._rhxPopoverInit = true;

      var triggerSel = popover.getAttribute("data-rhx-trigger");
      if (!triggerSel) return;

      var trigger;
      if (triggerSel === "previous") {
        trigger = popover.previousElementSibling;
      } else {
        trigger = document.querySelector(triggerSel);
      }
      if (!trigger) return;

      var arrowEl = popover.querySelector(".rhx-popover__arrow");
      var triggerEvent = popover.getAttribute("data-rhx-trigger-event") || "click";
      var hideTimer = null;
      var HOVER_DELAY = 100;

      function show() {
        clearTimeout(hideTimer);
        popover.hidden = false;
        popover.style.display = "block";
        popover.setAttribute("data-rhx-visible", "");
        popover.removeAttribute("aria-hidden");
        popover.classList.add("rhx-popover--open");

        reposition();

        trigger.setAttribute("aria-expanded", "true");

        popover.dispatchEvent(new CustomEvent("rhx:popover:show", {
          bubbles: true
        }));
      }

      function hide() {
        popover.removeAttribute("data-rhx-visible");
        popover.setAttribute("aria-hidden", "true");
        popover.classList.remove("rhx-popover--open");

        trigger.setAttribute("aria-expanded", "false");

        // Wait for transition
        setTimeout(function () {
          if (!popover.hasAttribute("data-rhx-visible")) {
            popover.hidden = true;
            popover.style.display = "";
          }
        }, 200);

        popover.dispatchEvent(new CustomEvent("rhx:popover:hide", {
          bubbles: true
        }));
      }

      function toggle() {
        if (popover.hasAttribute("data-rhx-visible")) {
          hide();
        } else {
          show();
        }
      }

      function isOpen() {
        return popover.hasAttribute("data-rhx-visible");
      }

      var useCssAnchoring = window.RHX && window.RHX.supportsAnchorPositioning;
      var cssAnchoringApplied = false;

      function reposition() {
        var placement = popover.getAttribute("data-rhx-placement") || "bottom";
        var distance = parseInt(popover.getAttribute("data-rhx-distance") || "8", 10);

        // Use CSS Anchor Positioning when supported (apply once, CSS handles the rest)
        if (useCssAnchoring && !cssAnchoringApplied) {
          window.RHX.applyCssAnchorPositioning(trigger, popover, {
            placement: placement,
            distance: distance
          });
          cssAnchoringApplied = true;
        }

        // In CSS anchor mode, position the arrow after the browser lays out the popover
        if (useCssAnchoring) {
          if (arrowEl) {
            // Use requestAnimationFrame to read geometry after CSS positions the popover
            requestAnimationFrame(function () {
              var anchorRect = trigger.getBoundingClientRect();
              var popoverRect = popover.getBoundingClientRect();
              var side = placement.split("-")[0];
              if (side === "top" || side === "bottom") {
                // Position arrow horizontally to align with trigger center
                var cx = anchorRect.left + anchorRect.width / 2 - popoverRect.left - 5;
                arrowEl.style.left = Math.max(8, Math.min(cx, popoverRect.width - 18)) + "px";
                // Position arrow vertically outside the popover
                if (side === "top") {
                  // Popover above trigger, arrow at bottom pointing down
                  arrowEl.style.top = (popoverRect.height - 5) + "px";
                } else {
                  // Popover below trigger, arrow at top pointing up
                  arrowEl.style.top = "-5px";
                }
              } else {
                // Position arrow vertically to align with trigger center
                var cy = anchorRect.top + anchorRect.height / 2 - popoverRect.top - 5;
                arrowEl.style.top = Math.max(8, Math.min(cy, popoverRect.height - 18)) + "px";
                // Position arrow horizontally outside the popover
                if (side === "left") {
                  // Popover left of trigger, arrow at right pointing right
                  arrowEl.style.left = (popoverRect.width - 5) + "px";
                } else {
                  // Popover right of trigger, arrow at left pointing left
                  arrowEl.style.left = "-5px";
                }
              }
            });
          }
          return;
        }

        // Fallback: JS positioning
        if (!window.RHX || !window.RHX.positionElement) return;

        window.RHX.positionElement(trigger, popover, {
          placement: placement,
          distance: distance,
          strategy: "absolute",
          flip: true,
          shift: true,
          arrowElement: arrowEl,
          arrowPadding: 8
        });
      }

      // Set up ARIA on trigger
      trigger.setAttribute("aria-haspopup", "dialog");
      trigger.setAttribute("aria-expanded", isOpen() ? "true" : "false");
      if (popover.id) {
        trigger.setAttribute("aria-controls", popover.id);
      }

      // ── Click trigger ──
      if (triggerEvent === "click") {
        trigger.addEventListener("click", function (e) {
          e.preventDefault();
          e.stopPropagation();
          toggle();
        });

        // Click outside to close
        document.addEventListener("click", function (e) {
          if (isOpen() && !popover.contains(e.target) && !trigger.contains(e.target)) {
            hide();
          }
        });

        // Escape to close
        document.addEventListener("keydown", function (e) {
          if (e.key === "Escape" && isOpen()) {
            hide();
            trigger.focus();
          }
        });
      }

      // ── Hover trigger ──
      if (triggerEvent === "hover") {
        trigger.addEventListener("mouseenter", function () { show(); });
        trigger.addEventListener("mouseleave", function () {
          hideTimer = setTimeout(function () {
            if (!popover.matches(":hover")) hide();
          }, HOVER_DELAY);
        });

        popover.addEventListener("mouseenter", function () {
          clearTimeout(hideTimer);
        });
        popover.addEventListener("mouseleave", function () {
          hideTimer = setTimeout(hide, HOVER_DELAY);
        });
      }

      // ── Focus trigger ──
      if (triggerEvent === "focus") {
        trigger.addEventListener("focusin", function () { show(); });
        trigger.addEventListener("focusout", function (e) {
          if (!popover.contains(e.relatedTarget)) {
            hide();
          }
        });
      }

      // Reposition on scroll/resize
      window.addEventListener("scroll", function () { if (isOpen()) reposition(); }, { passive: true });
      window.addEventListener("resize", function () { if (isOpen()) reposition(); }, { passive: true });

      // Handle server-rendered open state
      if (popover.classList.contains("rhx-popover--open")) {
        popover.hidden = false;
        popover.style.display = "block";
        popover.setAttribute("data-rhx-visible", "");
        popover.removeAttribute("aria-hidden");
        reposition();
      }
    });
  }

  if (window.RHX) {
    window.RHX.register("popover", initPopovers);
  }
})();
