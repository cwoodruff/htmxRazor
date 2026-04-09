/**
 * htmxRazor Tooltip
 * Show/hide tooltip popup on trigger events, positioned relative to target element.
 * Uses a singleton tooltip element for efficiency.
 */
(function () {
  "use strict";

  var tooltipEl = null;
  var arrowEl = null;
  var contentEl = null;
  var currentTarget = null;
  var tooltipId = "rhx-tooltip-popup";
  var GAP = 8;

  function ensureTooltip() {
    if (tooltipEl) return;
    tooltipEl = document.createElement("div");
    tooltipEl.className = "rhx-tooltip";
    tooltipEl.id = tooltipId;
    tooltipEl.setAttribute("role", "tooltip");

    contentEl = document.createElement("span");
    contentEl.className = "rhx-tooltip__content";
    tooltipEl.appendChild(contentEl);

    arrowEl = document.createElement("div");
    arrowEl.className = "rhx-tooltip__arrow";
    tooltipEl.appendChild(arrowEl);

    document.body.appendChild(tooltipEl);
  }

  function showTooltip(target) {
    var text = target.getAttribute("data-rhx-tooltip");
    if (!text || target.hasAttribute("data-rhx-tooltip-disabled")) return;

    ensureTooltip();
    currentTarget = target;
    contentEl.textContent = text;

    var placement = target.getAttribute("data-rhx-tooltip-placement") || "top";
    tooltipEl.setAttribute("data-placement", placement);

    // Set aria-describedby on the first focusable child or the target itself
    var focusable = target.querySelector("button, a, input, [tabindex]") || target;
    focusable.setAttribute("aria-describedby", tooltipId);

    // Show (need to display first for measurement)
    tooltipEl.classList.add("rhx-tooltip--visible");
    positionTooltip(target, placement);
  }

  function hideTooltip() {
    if (!tooltipEl || !currentTarget) return;
    tooltipEl.classList.remove("rhx-tooltip--visible");

    var focusable = currentTarget.querySelector("button, a, input, [tabindex]") || currentTarget;
    focusable.removeAttribute("aria-describedby");
    currentTarget = null;
  }

  var useCssAnchoring = window.RHX && window.RHX.supportsAnchorPositioning;

  function positionTooltip(target, placement) {
    // Get the first child element for positioning (since wrapper uses display: contents)
    var posTarget = target.firstElementChild || target;

    // Use CSS Anchor Positioning when supported
    if (useCssAnchoring) {
      window.RHX.applyCssAnchorPositioning(posTarget, tooltipEl, {
        placement: placement,
        distance: GAP
      });
      return;
    }

    // Fallback: JS positioning
    var rect = posTarget.getBoundingClientRect();
    var tipRect = tooltipEl.getBoundingClientRect();
    var top, left;

    switch (placement) {
      case "bottom":
        top = rect.bottom + GAP;
        left = rect.left + (rect.width - tipRect.width) / 2;
        break;
      case "left":
        top = rect.top + (rect.height - tipRect.height) / 2;
        left = rect.left - tipRect.width - GAP;
        break;
      case "right":
        top = rect.top + (rect.height - tipRect.height) / 2;
        left = rect.right + GAP;
        break;
      default: // top
        top = rect.top - tipRect.height - GAP;
        left = rect.left + (rect.width - tipRect.width) / 2;
        break;
    }

    // Keep within viewport
    left = Math.max(4, Math.min(left, window.innerWidth - tipRect.width - 4));
    top = Math.max(4, Math.min(top, window.innerHeight - tipRect.height - 4));

    tooltipEl.style.top = top + "px";
    tooltipEl.style.left = left + "px";
  }

  function initTooltips(root) {
    var wrappers = root.querySelectorAll("[data-rhx-tooltip]");
    wrappers.forEach(function (wrapper) {
      if (wrapper._rhxTooltipInit) return;
      wrapper._rhxTooltipInit = true;

      var trigger = wrapper.getAttribute("data-rhx-tooltip-trigger") || "hover focus";
      var triggers = trigger.split(/\s+/);

      // Get the actual interactive child (since wrapper is display: contents)
      var child = wrapper.firstElementChild || wrapper;

      if (triggers.indexOf("hover") >= 0) {
        child.addEventListener("mouseenter", function () { showTooltip(wrapper); });
        child.addEventListener("mouseleave", function () { hideTooltip(); });
      }

      if (triggers.indexOf("focus") >= 0) {
        child.addEventListener("focusin", function () { showTooltip(wrapper); });
        child.addEventListener("focusout", function () { hideTooltip(); });
      }

      if (triggers.indexOf("click") >= 0) {
        child.addEventListener("click", function (e) {
          if (currentTarget === wrapper) {
            hideTooltip();
          } else {
            hideTooltip();
            showTooltip(wrapper);
          }
        });
      }
    });
  }

  if (window.RHX) {
    window.RHX.register("tooltip", initTooltips);
  }
})();
