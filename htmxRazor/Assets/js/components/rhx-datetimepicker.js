/**
 * htmxRazor DateTime Picker
 * Popup with a calendar (left) and a time list (right). Selecting a day sets the date part;
 * selecting a time sets the time part; the control commits a hidden ISO yyyy-MM-ddTHH:mm value
 * once both are set, then closes. Calendar month nav is htmx-driven (the calendar pane swaps);
 * the JS only echoes server-rendered display text (no client re-formatting) and re-applies the
 * date highlight after a swap. Positioning reuses rhx-position.js.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";
  var TIME = ".rhx-time-picker__option:not([disabled])";

  function initDateTimePickers(root) {
    root.querySelectorAll("[data-rhx-datetime-picker]").forEach(function (dtp) {
      if (dtp._rhxDtInit) return;
      dtp._rhxDtInit = true;

      var input = dtp.querySelector("[data-rhx-dt-display]");
      var trigger = dtp.querySelector(".rhx-datetime-picker__trigger");
      var popup = dtp.querySelector(".rhx-datetime-picker__popup");
      var hidden = dtp.querySelector("[data-rhx-dt-value]");
      var times = dtp.querySelector(".rhx-datetime-picker__times");
      if (!input || !popup || !hidden) return;

      var dateIso = "", dateDisp = "", timeIso = "", timeDisp = "";

      // Guard: prevents the focus handler from re-opening immediately after selectDay/selectTime
      // calls close(true) + input.focus(). Set before input.focus(); cleared on the next tick
      // via setTimeout(0) so the flag outlives the focus event that fires synchronously.
      var suppressFocusOpen = false;

      (function seed() {
        var selDay = popup.querySelector(".rhx-calendar__day--selected");
        if (selDay) {
          dateIso = selDay.getAttribute("data-date") || "";
          dateDisp = selDay.getAttribute("data-display") || selDay.textContent.trim();
        }
        var selTime = popup.querySelector(".rhx-time-picker__option--selected");
        if (selTime) {
          timeIso = selTime.getAttribute("data-time") || "";
          timeDisp = selTime.textContent.trim();
        }
      })();

      function isOpen() { return !popup.hidden; }

      function open() {
        if (input.hasAttribute("disabled") || input.hasAttribute("readonly")) return;
        popup.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, popup, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
        var focusDay = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
        if (focusDay) focusDay.focus();
      }

      function close(focusInput) {
        popup.hidden = true;
        input.setAttribute("aria-expanded", "false");
        if (trigger) trigger.setAttribute("aria-expanded", "false");
        if (focusInput) {
          // Set the suppress flag before calling input.focus() so that the focus event
          // (which fires synchronously) sees it and does not re-open the popup.
          suppressFocusOpen = true;
          input.focus();
          // Clear on the next tick — after the synchronous focus event has fired.
          setTimeout(function () { suppressFocusOpen = false; }, 0);
        }
      }

      function refresh() {
        var committed = (dateIso && timeIso) ? dateIso + "T" + timeIso : "";
        input.value = (dateIso && timeIso)
          ? (dateDisp + " " + timeDisp)
          : [dateDisp, timeDisp].filter(Boolean).join(" ");
        if (committed !== hidden.value) {
          hidden.value = committed;
          hidden.dispatchEvent(new Event("input", { bubbles: true }));
          hidden.dispatchEvent(new Event("change", { bubbles: true }));
          dtp.dispatchEvent(new CustomEvent("rhx:datetime-picker:change", { bubbles: true, detail: { value: committed } }));
        }
      }

      function selectDay(cell) {
        var prev = popup.querySelector(".rhx-calendar__day--selected");
        if (prev) {
          prev.classList.remove("rhx-calendar__day--selected");
          prev.removeAttribute("aria-selected");
        }
        cell.classList.add("rhx-calendar__day--selected");
        cell.setAttribute("aria-selected", "true");
        dateIso = cell.getAttribute("data-date") || "";
        dateDisp = cell.getAttribute("data-display") || cell.textContent.trim();
        refresh();
        if (timeIso) close(true);
      }

      function selectTime(opt) {
        if (times) {
          var prev = times.querySelector(".rhx-time-picker__option--selected");
          if (prev) {
            prev.classList.remove("rhx-time-picker__option--selected");
            prev.removeAttribute("aria-selected");
          }
        }
        opt.classList.add("rhx-time-picker__option--selected");
        opt.setAttribute("aria-selected", "true");
        timeIso = opt.getAttribute("data-time") || "";
        timeDisp = opt.textContent.trim();
        refresh();
        if (dateIso) close(true);
      }

      // Trigger click: toggle. The suppressFocusOpen flag is NOT set here because we are
      // not committing — we just open or close explicitly, which is safe.
      if (trigger) {
        trigger.addEventListener("click", function () {
          if (isOpen()) {
            close(false);
          } else {
            open();
          }
        });
      }

      // Open on input focus — gated by the suppressFocusOpen guard so that
      // close(true) + input.focus() does not immediately re-open the popup.
      input.addEventListener("focus", function () {
        if (!suppressFocusOpen && !isOpen()) open();
      });

      // Delegated click on the stable popup element — survives htmx calendar swaps because
      // the listener is on popup (which is never swapped), not on individual day cells.
      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { selectDay(day); return; }

        var t = e.target.closest(TIME);
        if (t && times && times.contains(t)) { selectTime(t); return; }

        if (e.target.closest("[data-rhx-dt-clear]")) {
          dateIso = dateDisp = timeIso = timeDisp = "";
          var sd = popup.querySelector(".rhx-calendar__day--selected");
          if (sd) { sd.classList.remove("rhx-calendar__day--selected"); sd.removeAttribute("aria-selected"); }
          var st = times && times.querySelector(".rhx-time-picker__option--selected");
          if (st) { st.classList.remove("rhx-time-picker__option--selected"); st.removeAttribute("aria-selected"); }
          refresh();
          return;
        }

        if (e.target.closest("[data-rhx-dt-done]")) { close(true); }
      });

      function clickNav(sel) { var b = popup.querySelector(".rhx-calendar__nav" + sel); if (b) b.click(); }

      // Calendar grid keyboard (APG): arrows move (skipping disabled), PageUp/Down change month,
      // Enter/Space select the focused day, Escape closes. Mirrors the standalone date picker.
      popup.addEventListener("keydown", function (e) {
        var cur = document.activeElement;
        if (!cur || !cur.classList || !cur.classList.contains("rhx-calendar__day")) {
          if (e.key === "Escape") { e.preventDefault(); close(true); }
          return;
        }
        var days = Array.prototype.slice.call(popup.querySelectorAll(".rhx-calendar__day"));
        var i = days.indexOf(cur);
        var to = null, dir = 1, navOverflow = true;
        switch (e.key) {
          case "ArrowRight": to = i + 1; dir = 1; break;
          case "ArrowLeft": to = i - 1; dir = -1; break;
          case "ArrowDown": to = i + 7; dir = 1; break;
          case "ArrowUp": to = i - 7; dir = -1; break;
          case "Home": to = i - (i % 7); dir = 1; navOverflow = false; break;
          case "End": to = i - (i % 7) + 6; dir = -1; navOverflow = false; break;
          case "PageUp": e.preventDefault(); clickNav("[aria-label='Previous month']"); return;
          case "PageDown": e.preventDefault(); clickNav("[aria-label='Next month']"); return;
          case "Enter": case " ":
            e.preventDefault();
            if (!cur.hasAttribute("disabled")) selectDay(cur);
            return;
          case "Escape": e.preventDefault(); close(true); return;
          default: return;
        }
        e.preventDefault();
        while (to >= 0 && to < days.length && days[to].hasAttribute("disabled")) { to += dir; }
        if (to < 0 || to >= days.length) {
          if (navOverflow) clickNav(dir < 0 ? "[aria-label='Previous month']" : "[aria-label='Next month']");
          return;
        }
        days.forEach(function (d) { d.setAttribute("tabindex", "-1"); });
        days[to].setAttribute("tabindex", "0");
        days[to].focus();
      });

      // After an htmx calendar swap, re-apply the client's date selection to whichever
      // cell now carries that date (if it is visible in the freshly swapped grid).
      popup.addEventListener("htmx:afterSwap", function () {
        if (dateIso) {
          var cell = popup.querySelector('.rhx-calendar__day[data-date="' + dateIso + '"]');
          if (cell) {
            var prev = popup.querySelector(".rhx-calendar__day--selected");
            if (prev && prev !== cell) { prev.classList.remove("rhx-calendar__day--selected"); prev.removeAttribute("aria-selected"); }
            cell.classList.add("rhx-calendar__day--selected");
            cell.setAttribute("aria-selected", "true");
          }
        }
        if (!popup.hidden) {
          var f = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
          if (f) f.focus();
        }
      });

      document.addEventListener("click", function (e) {
        if (isOpen() && !dtp.contains(e.target)) close(false);
      });
    });
  }

  if (window.RHX) window.RHX.register("datetime-picker", initDateTimePickers);
})();
