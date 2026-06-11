/**
 * htmxRazor Radial Select
 * A pie of category wedges opened from a rectangular trigger. Selecting a wedge echoes its
 * color + icon onto the trigger, sets the hidden category input, and fires the wedge's
 * cascade request via htmx.ajax to repopulate the dropdown listbox — then auto-selects the
 * first option. The dropdown is a combobox: its button shows the selected value and opens a
 * popup listbox of the active category's items. The pie opens centered over the trigger.
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
      var combobox = control.querySelector(".rhx-radial-select__combobox");
      var listbox = control.querySelector(".rhx-radial-select__listbox");
      var display = control.querySelector("[data-rhx-radial-display]");
      var categoryInput = control.querySelector("[data-rhx-radial-category]");
      var valueInput = control.querySelector("[data-rhx-radial-value]");
      if (!trigger || !pie) return;

      var wedges = Array.prototype.slice.call(pie.querySelectorAll(WEDGE_SELECTOR));

      // Reflect the initial empty/placeholder state on the value display.
      if (display && (!valueInput || !valueInput.value)) {
        display.setAttribute("data-rhx-empty", "");
      }

      // ──────────────────────────────────────────────
      //  Pie (category picker)
      // ──────────────────────────────────────────────

      function pieOpen() {
        return !pie.hidden;
      }

      function openPie() {
        if (trigger.hasAttribute("disabled")) return;
        closeListbox();
        pie.hidden = false;
        trigger.setAttribute("aria-expanded", "true");
        centerPieOverTrigger();
        var active = wedges.find(function (w) {
          return w.getAttribute("aria-checked") === "true";
        }) || wedges[0];
        focusWedge(active);
      }

      // Center the circular pie directly over the trigger (its center aligns with the
      // trigger's center), so it pops out of the button like a radial menu.
      function centerPieOverTrigger() {
        var parent = pie.offsetParent || control;
        var parentRect = parent.getBoundingClientRect();
        var trigRect = trigger.getBoundingClientRect();
        var centerX = trigRect.left + trigRect.width / 2;
        var centerY = trigRect.top + trigRect.height / 2;
        pie.style.right = "auto";
        pie.style.bottom = "auto";
        pie.style.left = (centerX - parentRect.left - pie.offsetWidth / 2) + "px";
        pie.style.top = (centerY - parentRect.top - pie.offsetHeight / 2) + "px";
      }

      function closePie(focusTrigger) {
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

      function selectWedge(wedge) {
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

        closePie(false);
      }

      // ──────────────────────────────────────────────
      //  Dropdown (items within the active category)
      // ──────────────────────────────────────────────

      function listboxOpen() {
        return listbox && !listbox.hidden;
      }

      function openListbox() {
        if (!listbox || combobox.hasAttribute("disabled")) return;
        if (!listbox.querySelector(OPTION_SELECTOR)) return; // nothing to show yet
        closePie(false);
        listbox.hidden = false;
        combobox.setAttribute("aria-expanded", "true");
        var sel = listbox.querySelector('[role="option"][aria-selected="true"]')
          || listbox.querySelector(OPTION_SELECTOR);
        focusOption(sel);
      }

      function closeListbox() {
        if (!listbox) return;
        listbox.hidden = true;
        if (combobox) combobox.setAttribute("aria-expanded", "false");
        var f = listbox.querySelector("[data-rhx-focused]");
        if (f) f.removeAttribute("data-rhx-focused");
      }

      function getOptions() {
        return Array.prototype.slice.call(listbox.querySelectorAll(OPTION_SELECTOR));
      }

      function focusOption(option) {
        if (!option) return;
        var prev = listbox.querySelector("[data-rhx-focused]");
        if (prev) prev.removeAttribute("data-rhx-focused");
        option.setAttribute("data-rhx-focused", "");
        option.scrollIntoView({ block: "nearest" });
      }

      function moveOption(delta) {
        var opts = getOptions();
        if (!opts.length) return;
        var cur = listbox.querySelector("[data-rhx-focused]");
        var idx = cur ? opts.indexOf(cur) : -1;
        var next = (idx + delta + opts.length) % opts.length;
        focusOption(opts[next]);
      }

      function selectOption(option) {
        if (!option) return;
        listbox.querySelectorAll('[aria-selected="true"]').forEach(function (o) {
          o.setAttribute("aria-selected", "false");
        });
        option.setAttribute("aria-selected", "true");
        setDisplay(option.textContent.trim());
        if (valueInput) {
          valueInput.value = option.getAttribute("data-value") || "";
          valueInput.dispatchEvent(new Event("change", { bubbles: true }));
        }
      }

      function setDisplay(text) {
        if (!display) return;
        if (text) {
          display.textContent = text;
          display.removeAttribute("data-rhx-empty");
        } else {
          display.textContent = display.getAttribute("data-rhx-placeholder") || "";
          display.setAttribute("data-rhx-empty", "");
        }
      }

      function autoSelectFirst() {
        if (!listbox) return;
        var first = listbox.querySelector(OPTION_SELECTOR);
        listbox.querySelectorAll('[aria-selected="true"]').forEach(function (o) {
          o.setAttribute("aria-selected", "false");
        });
        if (first) {
          first.setAttribute("aria-selected", "true");
          setDisplay(first.textContent.trim());
          if (valueInput) valueInput.value = first.getAttribute("data-value") || "";
        } else {
          setDisplay("");
          if (valueInput) valueInput.value = "";
        }
        if (valueInput) valueInput.dispatchEvent(new Event("change", { bubbles: true }));
      }

      // ──────────────────────────────────────────────
      //  Events — pie
      // ──────────────────────────────────────────────

      trigger.addEventListener("click", function () {
        pieOpen() ? closePie(false) : openPie();
      });

      trigger.addEventListener("keydown", function (e) {
        if ((e.key === "ArrowDown" || e.key === "Enter" || e.key === " ") && !pieOpen()) {
          e.preventDefault();
          openPie();
        }
      });

      pie.addEventListener("click", function (e) {
        var wedge = e.target.closest(WEDGE_SELECTOR);
        if (wedge && pie.contains(wedge)) selectWedge(wedge);
      });

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
            if (idx >= 0) selectWedge(wedges[idx]);
            break;
          case "Escape":
            e.preventDefault();
            closePie(true);
            break;
          case "Tab":
            closePie(false);
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

      // ──────────────────────────────────────────────
      //  Events — dropdown
      // ──────────────────────────────────────────────

      if (combobox && listbox) {
        combobox.addEventListener("click", function () {
          listboxOpen() ? closeListbox() : openListbox();
        });

        // Options aren't focusable, so all dropdown keys are handled on the combobox button.
        combobox.addEventListener("keydown", function (e) {
          if (!listboxOpen()) {
            if (e.key === "ArrowDown" || e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              openListbox();
            }
            return;
          }
          switch (e.key) {
            case "ArrowDown":
              e.preventDefault();
              moveOption(1);
              break;
            case "ArrowUp":
              e.preventDefault();
              moveOption(-1);
              break;
            case "Home":
              e.preventDefault();
              focusOption(getOptions()[0]);
              break;
            case "End":
              e.preventDefault();
              var opts = getOptions();
              focusOption(opts[opts.length - 1]);
              break;
            case "Enter":
            case " ":
              e.preventDefault();
              var focused = listbox.querySelector("[data-rhx-focused]");
              if (focused) {
                selectOption(focused);
                closeListbox();
              }
              break;
            case "Escape":
              e.preventDefault();
              closeListbox();
              break;
            case "Tab":
              closeListbox();
              break;
          }
        });

        listbox.addEventListener("click", function (e) {
          var option = e.target.closest(OPTION_SELECTOR);
          if (!option || !listbox.contains(option)) return;
          selectOption(option);
          closeListbox();
          combobox.focus();
        });
      }

      // ── Click outside closes both popups ──
      document.addEventListener("click", function (e) {
        if (control.contains(e.target)) return;
        if (pieOpen()) closePie(false);
        if (listboxOpen()) closeListbox();
      });
    });
  }

  if (window.RHX) {
    window.RHX.register("radial-select", initRadialSelects);
  }
})();
