/**
 * htmxRazor Positioning Engine
 * Shared positioning logic for popup, popover, tooltip, dropdown, and select.
 * Calculates position relative to an anchor element with flip and shift support.
 */
(function () {
  "use strict";

  var RHX = window.RHX || {};

  /**
   * Whether the browser supports CSS Anchor Positioning.
   * When true, components can skip JS positioning and rely on CSS.
   */
  RHX.supportsAnchorPositioning = typeof CSS !== "undefined" && CSS.supports && CSS.supports("anchor-name", "--x");

  /**
   * Map a placement string (e.g. "bottom-start") to a CSS position-area value.
   * @param {string} placement
   * @returns {string}
   */
  RHX.placementToPositionArea = function (placement) {
    var map = {
      "top": "top center",
      "top-start": "top left",
      "top-end": "top right",
      "bottom": "bottom center",
      "bottom-start": "bottom left",
      "bottom-end": "bottom right",
      "left": "left center",
      "left-start": "top left",
      "left-end": "bottom left",
      "right": "right center",
      "right-start": "top right",
      "right-end": "bottom right"
    };
    return map[placement] || "bottom center";
  };

  /**
   * Apply CSS Anchor Positioning styles to an anchor + floating element pair.
   * Call this once during init when RHX.supportsAnchorPositioning is true.
   *
   * @param {Element} anchor   - The trigger/reference element
   * @param {Element} floating - The positioned element
   * @param {Object}  options
   * @param {string}  options.placement - e.g. "bottom-start"
   * @param {number}  options.distance  - Gap in pixels (applied as margin)
   */
  RHX.applyCssAnchorPositioning = function (anchor, floating, options) {
    options = options || {};
    var placement = options.placement || "bottom";
    var distance = options.distance != null ? options.distance : 4;

    // Generate a unique anchor name
    var anchorName = anchor.getAttribute("data-rhx-anchor-name");
    if (!anchorName) {
      anchorName = "--rhx-anchor-" + Math.random().toString(36).substr(2, 8);
      anchor.setAttribute("data-rhx-anchor-name", anchorName);
    }

    anchor.style.anchorName = anchorName;
    floating.style.positionAnchor = anchorName;
    floating.style.position = "fixed";
    floating.style.positionArea = RHX.placementToPositionArea(placement);
    floating.style.margin = distance + "px";
    floating.style.positionTryFallbacks = "flip-block, flip-inline";
  };

  /**
   * Compute the position of a floating element relative to an anchor.
   *
   * @param {Element} anchor   - The reference element
   * @param {Element} floating - The element to position
   * @param {Object}  options  - Positioning options
   * @param {string}  options.placement  - e.g. "bottom-start", "top", "right-end"
   * @param {number}  options.distance   - Gap along main axis (default 4)
   * @param {number}  options.skidding   - Offset along cross axis (default 0)
   * @param {boolean} options.flip       - Flip if overflowing (default true)
   * @param {boolean} options.shift      - Shift to stay in bounds (default true)
   * @param {string}  options.strategy   - "absolute" or "fixed" (default "absolute")
   * @returns {{ x: number, y: number, placement: string, arrowX: number|null, arrowY: number|null }}
   */
  RHX.computePosition = function (anchor, floating, options) {
    options = options || {};
    var placement = options.placement || "bottom-start";
    var distance = options.distance != null ? options.distance : 4;
    var skidding = options.skidding || 0;
    var doFlip = options.flip !== false;
    var doShift = options.shift !== false;
    var strategy = options.strategy || "absolute";

    var anchorRect = anchor.getBoundingClientRect();
    var floatingRect = floating.getBoundingClientRect();
    var viewW = window.innerWidth;
    var viewH = window.innerHeight;

    // Parse placement into side + alignment
    var parts = placement.split("-");
    var side = parts[0];
    var align = parts[1] || "center";

    // Compute initial position
    var coords = calcCoords(anchorRect, floatingRect, side, align, distance, skidding, strategy);

    // Flip if overflowing
    if (doFlip) {
      var flipped = maybeFlip(coords, floatingRect, side, align, anchorRect, distance, skidding, viewW, viewH, strategy);
      if (flipped) {
        coords = flipped.coords;
        side = flipped.side;
        placement = align === "center" ? side : side + "-" + align;
      }
    }

    // Shift to keep in viewport
    if (doShift) {
      coords = applyShift(coords, floatingRect, viewW, viewH);
    }

    // Arrow position (centered on anchor along cross axis)
    var arrowX = null;
    var arrowY = null;
    if (options.arrow) {
      var arrowSize = options.arrowSize || 10;
      var arrowPadding = options.arrowPadding || 8;
      var arrowResult = calcArrow(anchorRect, floatingRect, coords, side, arrowSize, arrowPadding, strategy);
      arrowX = arrowResult.x;
      arrowY = arrowResult.y;
    }

    return {
      x: Math.round(coords.x),
      y: Math.round(coords.y),
      placement: placement,
      arrowX: arrowX != null ? Math.round(arrowX) : null,
      arrowY: arrowY != null ? Math.round(arrowY) : null
    };
  };

  function calcCoords(anchorRect, floatingRect, side, align, distance, skidding, strategy) {
    var x, y;
    var offsetX = strategy === "absolute" ? window.scrollX : 0;
    var offsetY = strategy === "absolute" ? window.scrollY : 0;

    // Main axis
    switch (side) {
      case "top":
        y = anchorRect.top - floatingRect.height - distance + offsetY;
        break;
      case "bottom":
        y = anchorRect.bottom + distance + offsetY;
        break;
      case "left":
        x = anchorRect.left - floatingRect.width - distance + offsetX;
        break;
      case "right":
        x = anchorRect.right + distance + offsetX;
        break;
    }

    // Cross axis
    if (side === "top" || side === "bottom") {
      switch (align) {
        case "start":
          x = anchorRect.left + skidding + offsetX;
          break;
        case "end":
          x = anchorRect.right - floatingRect.width - skidding + offsetX;
          break;
        default: // center
          x = anchorRect.left + (anchorRect.width - floatingRect.width) / 2 + skidding + offsetX;
          break;
      }
    } else {
      switch (align) {
        case "start":
          y = anchorRect.top + skidding + offsetY;
          break;
        case "end":
          y = anchorRect.bottom - floatingRect.height - skidding + offsetY;
          break;
        default: // center
          y = anchorRect.top + (anchorRect.height - floatingRect.height) / 2 + skidding + offsetY;
          break;
      }
    }

    return { x: x, y: y };
  }

  function maybeFlip(coords, floatingRect, side, align, anchorRect, distance, skidding, viewW, viewH, strategy) {
    var x = coords.x - (strategy === "absolute" ? window.scrollX : 0);
    var y = coords.y - (strategy === "absolute" ? window.scrollY : 0);

    var overflow = false;
    var oppSide;

    switch (side) {
      case "bottom":
        overflow = y + floatingRect.height > viewH;
        oppSide = "top";
        break;
      case "top":
        overflow = y < 0;
        oppSide = "bottom";
        break;
      case "right":
        overflow = x + floatingRect.width > viewW;
        oppSide = "left";
        break;
      case "left":
        overflow = x < 0;
        oppSide = "right";
        break;
    }

    if (overflow) {
      var newCoords = calcCoords(anchorRect, floatingRect, oppSide, align, distance, skidding, strategy);
      return { coords: newCoords, side: oppSide };
    }
    return null;
  }

  function applyShift(coords, floatingRect, viewW, viewH) {
    var pad = 4;
    var x = coords.x;
    var y = coords.y;
    var scrollX = window.scrollX;
    var scrollY = window.scrollY;

    // Clamp to viewport
    x = Math.max(pad + scrollX, Math.min(x, viewW - floatingRect.width - pad + scrollX));
    y = Math.max(pad + scrollY, Math.min(y, viewH - floatingRect.height - pad + scrollY));

    return { x: x, y: y };
  }

  function calcArrow(anchorRect, floatingRect, coords, side, arrowSize, arrowPadding, strategy) {
    var halfArrow = arrowSize / 2;
    var offsetX = strategy === "absolute" ? window.scrollX : 0;
    var offsetY = strategy === "absolute" ? window.scrollY : 0;
    var arrowX = null;
    var arrowY = null;

    if (side === "top" || side === "bottom") {
      // Arrow along x-axis
      var anchorCenterX = anchorRect.left + anchorRect.width / 2 + offsetX;
      arrowX = anchorCenterX - coords.x - halfArrow;
      arrowX = Math.max(arrowPadding, Math.min(arrowX, floatingRect.width - arrowSize - arrowPadding));

      if (side === "top") {
        arrowY = floatingRect.height - halfArrow;
      } else {
        arrowY = -halfArrow;
      }
    } else {
      // Arrow along y-axis
      var anchorCenterY = anchorRect.top + anchorRect.height / 2 + offsetY;
      arrowY = anchorCenterY - coords.y - halfArrow;
      arrowY = Math.max(arrowPadding, Math.min(arrowY, floatingRect.height - arrowSize - arrowPadding));

      if (side === "left") {
        arrowX = floatingRect.width - halfArrow;
      } else {
        arrowX = -halfArrow;
      }
    }

    return { x: arrowX, y: arrowY };
  }

  /**
   * Position a floating element relative to its anchor and apply styles.
   *
   * @param {Element} anchor
   * @param {Element} floating
   * @param {Object}  options
   */
  RHX.positionElement = function (anchor, floating, options) {
    options = options || {};
    var arrowEl = options.arrowElement || null;

    // Temporarily show for measurement
    var wasHidden = floating.hidden;
    if (wasHidden) {
      floating.style.visibility = "hidden";
      floating.hidden = false;
      floating.style.display = "block";
    }

    var strategy = options.strategy || "absolute";

    var result = RHX.computePosition(anchor, floating, {
      placement: options.placement,
      distance: options.distance,
      skidding: options.skidding,
      flip: options.flip,
      shift: options.shift,
      strategy: strategy,
      arrow: !!arrowEl,
      arrowSize: arrowEl ? arrowEl.offsetWidth : 10,
      arrowPadding: options.arrowPadding || 8
    });

    var x = result.x;
    var y = result.y;

    // For absolute positioning, convert page-absolute coords to offsetParent-relative
    if (strategy === "absolute") {
      var offsetParent = floating.offsetParent || document.body;
      var parentRect = offsetParent.getBoundingClientRect();
      x -= (parentRect.left + window.scrollX);
      y -= (parentRect.top + window.scrollY);
    }

    floating.style.left = x + "px";
    floating.style.top = y + "px";
    floating.setAttribute("data-rhx-current-placement", result.placement);

    if (arrowEl) {
      if (result.arrowX != null) arrowEl.style.left = result.arrowX + "px";
      if (result.arrowY != null) arrowEl.style.top = result.arrowY + "px";
    }

    if (wasHidden) {
      floating.style.visibility = "";
    }

    return result;
  };

  window.RHX = RHX;
})();
