/**
 * htmxRazor Dropdown
 * Open/close toggling, keyboard navigation (Arrow keys, Home/End, Enter/Space, Escape, Tab),
 * click-outside dismissal, viewport flip detection, checkbox toggling, stay-open support,
 * focus management, and ARIA state management.
 */
(function () {
  "use strict";

  var ITEM_SELECTOR =
    "[role='menuitem']:not([disabled]):not(.rhx-dropdown__item--disabled)," +
    "[role='menuitemcheckbox']:not([disabled]):not(.rhx-dropdown__item--disabled)";

  function initDropdowns(root) {
    var dropdowns = root.querySelectorAll("[data-rhx-dropdown]");
    dropdowns.forEach(function (dropdown) {
      if (dropdown._rhxDropdownInit) return;
      dropdown._rhxDropdownInit = true;

      var triggerWrapper = dropdown.querySelector("[data-rhx-dropdown-trigger]");
      var menu = dropdown.querySelector("[role='menu']");
      if (!triggerWrapper || !menu) return;

      var stayOpen = dropdown.hasAttribute("data-rhx-stay-open");

      // Find the actual focusable trigger element (button/a inside the wrapper)
      var triggerBtn = triggerWrapper.querySelector("button, a, [tabindex]") || triggerWrapper;

      // Transfer ARIA attributes from wrapper to actual button
      if (triggerBtn !== triggerWrapper) {
        var attrs = ["aria-expanded", "aria-controls", "aria-haspopup"];
        attrs.forEach(function (attr) {
          var val = triggerWrapper.getAttribute(attr);
          if (val) triggerBtn.setAttribute(attr, val);
        });
      }

      function getItems() {
        return Array.from(menu.querySelectorAll(ITEM_SELECTOR));
      }

      function open() {
        menu.hidden = false;
        menu.setAttribute("aria-hidden", "false");
        triggerBtn.setAttribute("aria-expanded", "true");
        checkViewportFlip();
        var first = menu.querySelector(ITEM_SELECTOR);
        if (first) first.focus();
      }

      function close(restoreFocus) {
        menu.hidden = true;
        menu.setAttribute("aria-hidden", "true");
        triggerBtn.setAttribute("aria-expanded", "false");
        dropdown.removeAttribute("data-rhx-flipped");
        if (restoreFocus !== false) triggerBtn.focus();
      }

      function isOpen() {
        return !menu.hidden;
      }

      function toggle() {
        isOpen() ? close() : open();
      }

      // Viewport flip detection
      function checkViewportFlip() {
        var placement = dropdown.getAttribute("data-rhx-placement") || "bottom-start";
        var rect = menu.getBoundingClientRect();
        var viewportHeight = window.innerHeight;

        dropdown.removeAttribute("data-rhx-flipped");

        if (placement.startsWith("bottom") && rect.bottom > viewportHeight) {
          dropdown.setAttribute("data-rhx-flipped", "");
        } else if (placement.startsWith("top") && rect.top < 0) {
          dropdown.setAttribute("data-rhx-flipped", "");
        }
      }

      // Handle item activation
      function activateItem(item) {
        // Checkbox toggle
        if (item.getAttribute("role") === "menuitemcheckbox") {
          var isChecked = item.getAttribute("aria-checked") === "true";
          item.setAttribute("aria-checked", (!isChecked).toString());
          item.classList.toggle("rhx-dropdown__item--checked");

          var checkEl = item.querySelector(".rhx-dropdown__item-check");
          if (checkEl) {
            checkEl.innerHTML = isChecked ? "" : "&#10003;";
          }

          item.dispatchEvent(new CustomEvent("rhx:dropdown:check", {
            bubbles: true,
            detail: { value: item.getAttribute("data-value"), checked: !isChecked }
          }));

          // Checkboxes typically stay open
          return;
        }

        // Normal item: dispatch event and close
        item.dispatchEvent(new CustomEvent("rhx:dropdown:select", {
          bubbles: true,
          detail: { value: item.getAttribute("data-value") }
        }));

        if (!stayOpen) {
          close();
        }
      }

      // ── Trigger click ──
      triggerBtn.addEventListener("click", function (e) {
        e.preventDefault();
        toggle();
      });

      // ── Trigger keyboard ──
      triggerBtn.addEventListener("keydown", function (e) {
        if (e.key === "ArrowDown") {
          e.preventDefault();
          if (!isOpen()) {
            open(); // open() already focuses first item
          }
        } else if (e.key === "ArrowUp") {
          e.preventDefault();
          if (!isOpen()) {
            // APG: ArrowUp on trigger opens menu and focuses last item
            menu.hidden = false;
            menu.setAttribute("aria-hidden", "false");
            triggerBtn.setAttribute("aria-expanded", "true");
            checkViewportFlip();
            var items = getItems();
            if (items.length) items[items.length - 1].focus();
          }
        } else if (e.key === "Escape" && isOpen()) {
          e.preventDefault();
          close();
        }
      });

      // ── Menu keyboard navigation ──
      menu.addEventListener("keydown", function (e) {
        var items = getItems();
        var idx = items.indexOf(document.activeElement);

        if (e.key === "ArrowDown") {
          e.preventDefault();
          items[(idx + 1) % items.length].focus();
        } else if (e.key === "ArrowUp") {
          e.preventDefault();
          items[(idx - 1 + items.length) % items.length].focus();
        } else if (e.key === "Escape") {
          e.preventDefault();
          close();
        } else if (e.key === "Home") {
          e.preventDefault();
          if (items.length) items[0].focus();
        } else if (e.key === "End") {
          e.preventDefault();
          if (items.length) items[items.length - 1].focus();
        } else if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          if (idx >= 0) activateItem(items[idx]);
        } else if (e.key === "Tab") {
          close();
        } else if (e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey) {
          // APG: type-ahead — printable character moves focus to next matching item
          e.preventDefault();
          var ch = e.key.toLowerCase();
          for (var i = 1; i <= items.length; i++) {
            var candidate = items[(idx + i) % items.length];
            var text = candidate.textContent.trim().toLowerCase();
            if (text.charAt(0) === ch) {
              candidate.focus();
              break;
            }
          }
        }
      });

      // ── Item click delegation ──
      menu.addEventListener("click", function (e) {
        var item = e.target.closest(ITEM_SELECTOR);
        if (item && menu.contains(item)) {
          activateItem(item);
        }
      });

      // ── Click outside ──
      document.addEventListener("click", function (e) {
        if (isOpen() && !dropdown.contains(e.target)) {
          close(false);
        }
      });

      // ── Server-rendered open state ──
      if (dropdown.classList.contains("rhx-dropdown--open")) {
        menu.hidden = false;
        menu.setAttribute("aria-hidden", "false");
        triggerBtn.setAttribute("aria-expanded", "true");
        checkViewportFlip();
      }
    });
  }

  if (window.RHX) {
    window.RHX.register("dropdown", initDropdowns);
  }
})();
