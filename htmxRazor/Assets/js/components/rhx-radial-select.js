/**
 * htmxRazor Radial Select
 * A pie of category wedges opened from a rectangular trigger. Selecting a wedge echoes its
 * color + icon onto the trigger, sets the hidden category input, and fires the wedge's
 * cascade request via htmx.ajax to repopulate the dropdown listbox — then auto-selects the
 * first option. Positioning reuses rhx-position.js.
 */
(function () {
  "use strict";

  var WEDGE_SELECTOR = '[role="menuitemradio"]';
  var OPTION_SELECTOR = '[role="option"]:not([aria-disabled="true"])';

  function initRadialSelects(root) {
    var controls = root.querySelectorAll("[data-rhx-radial-select]");
    controls.forEach(function (control) {
      if (control._rhxRadialInit) return;
      control._rhxRadialInit = true;

      var trigger = control.querySelector(".rhx-radial-select__trigger");
      var pie = control.querySelector(".rhx-radial-select__pie");
      var listbox = control.querySelector(".rhx-radial-select__listbox");
      var categoryInput = control.querySelector("[data-rhx-radial-category]");
      var valueInput = control.querySelector("[data-rhx-radial-value]");
      if (!trigger || !pie) return;

      var wedges = Array.prototype.slice.call(pie.querySelectorAll(WEDGE_SELECTOR));

      function isOpen() {
        return !pie.hidden;
      }

      function open() {
        if (trigger.hasAttribute("disabled")) return;
        pie.hidden = false;
        trigger.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(trigger, pie, {
            placement: "bottom-start",
            distance: 6,
            flip: true,
            shift: true,
          });
        }
        var active = wedges.find(function (w) {
          return w.getAttribute("aria-checked") === "true";
        }) || wedges[0];
        focusWedge(active);
      }

      function close(focusTrigger) {
        pie.hidden = true;
        trigger.setAttribute("aria-expanded", "false");
        if (focusTrigger) trigger.focus();
      }

      function focusWedge(wedge) {
        if (!wedge) return;
        wedges.forEach(function (w) { w.setAttribute("tabindex", "-1"); });
        wedge.setAttribute("tabindex", "0");
        wedge.focus();
      }

      function select(wedge) {
        if (!wedge || wedge.getAttribute("aria-disabled") === "true") return;

        wedges.forEach(function (w) { w.setAttribute("aria-checked", "false"); });
        wedge.setAttribute("aria-checked", "true");

        var value = wedge.getAttribute("data-rhx-radial-option-value") || "";
        var color = wedge.getAttribute("data-rhx-radial-color");
        var hxGet = wedge.getAttribute("data-rhx-radial-hx-get");

        if (categoryInput) {
          categoryInput.value = value;
          categoryInput.dispatchEvent(new Event("change", { bubbles: true }));
        }

        // Echo color + icon onto the trigger.
        if (color) {
          trigger.setAttribute("data-rhx-active-color", color);
          trigger.style.setProperty("--rhx-radial-active", "var(--rhx-color-" + color + "-500)");
        }
        var iconG = wedge.querySelector(".rhx-radial-select__wedge-icon");
        var triggerIcon = trigger.querySelector(".rhx-radial-select__trigger-icon");
        if (!triggerIcon) {
          triggerIcon = document.createElement("span");
          triggerIcon.className = "rhx-radial-select__trigger-icon";
          triggerIcon.setAttribute("aria-hidden", "true");
          trigger.appendChild(triggerIcon);
        }
        triggerIcon.innerHTML = iconG ? iconG.innerHTML : "";

        control.dispatchEvent(new CustomEvent("rhx:radial-select:change", {
          bubbles: true,
          detail: { value: value },
        }));

        // Fire the cascade via htmx; auto-select first when it lands.
        if (hxGet && window.htmx) {
          window.htmx
            .ajax("GET", hxGet, { target: listbox, swap: "innerHTML" })
            .then(autoSelectFirst);
        }

        close(false);
      }

      function autoSelectFirst() {
        if (!listbox) return;
        var first = listbox.querySelector(OPTION_SELECTOR);
        listbox.querySelectorAll('[aria-selected="true"]').forEach(function (o) {
          o.setAttribute("aria-selected", "false");
        });
        if (first) {
          first.setAttribute("aria-selected", "true");
          if (valueInput) valueInput.value = first.getAttribute("data-value") || "";
        } else if (valueInput) {
          valueInput.value = "";
        }
        if (valueInput) valueInput.dispatchEvent(new Event("change", { bubbles: true }));
      }

      // ── Trigger ──
      trigger.addEventListener("click", function () {
        isOpen() ? close(false) : open();
      });

      trigger.addEventListener("keydown", function (e) {
        if ((e.key === "ArrowDown" || e.key === "Enter" || e.key === " ") && !isOpen()) {
          e.preventDefault();
          open();
        }
      });

      // ── Wedge click ──
      pie.addEventListener("click", function (e) {
        var wedge = e.target.closest(WEDGE_SELECTOR);
        if (wedge && pie.contains(wedge)) select(wedge);
      });

      // ── Wedge keyboard (roving focus + type-ahead) ──
      pie.addEventListener("keydown", function (e) {
        if (!wedges.length) return;
        var idx = wedges.indexOf(document.activeElement);

        switch (e.key) {
          case "ArrowRight":
          case "ArrowDown":
            e.preventDefault();
            focusWedge(wedges[(idx + 1) % wedges.length]);
            break;
          case "ArrowLeft":
          case "ArrowUp":
            e.preventDefault();
            focusWedge(wedges[(idx - 1 + wedges.length) % wedges.length]);
            break;
          case "Home":
            e.preventDefault();
            focusWedge(wedges[0]);
            break;
          case "End":
            e.preventDefault();
            focusWedge(wedges[wedges.length - 1]);
            break;
          case "Enter":
          case " ":
            e.preventDefault();
            if (idx >= 0) select(wedges[idx]);
            break;
          case "Escape":
            e.preventDefault();
            close(true);
            break;
          case "Tab":
            close(false);
            break;
          default:
            if (e.key.length === 1) {
              var match = wedges.find(function (w) {
                return (w.getAttribute("aria-label") || "")
                  .toLowerCase()
                  .indexOf(e.key.toLowerCase()) === 0;
              });
              if (match) focusWedge(match);
            }
        }
      });

      // ── Listbox option selection (within the active category) ──
      if (listbox) {
        listbox.addEventListener("click", function (e) {
          var option = e.target.closest(OPTION_SELECTOR);
          if (!option || !listbox.contains(option)) return;
          listbox.querySelectorAll('[aria-selected="true"]').forEach(function (o) {
            o.setAttribute("aria-selected", "false");
          });
          option.setAttribute("aria-selected", "true");
          if (valueInput) {
            valueInput.value = option.getAttribute("data-value") || "";
            valueInput.dispatchEvent(new Event("change", { bubbles: true }));
          }
        });
      }

      // ── Click outside ──
      document.addEventListener("click", function (e) {
        if (isOpen() && !control.contains(e.target)) close(false);
      });
    });
  }

  if (window.RHX) {
    window.RHX.register("radial-select", initRadialSelects);
  }
})();
