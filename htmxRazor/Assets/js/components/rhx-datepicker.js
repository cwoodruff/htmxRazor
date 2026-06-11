/**
 * htmxRazor Date Picker
 * Opens a popup calendar; month/year navigation is htmx-driven (the grid swaps in).
 * Day selection is committed client-side: hidden ISO input + visible display + close.
 * Keyboard follows the APG grid pattern; Today/Clear footer actions are JS.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";

  function fmtDisplay(iso) {
    var p = iso.split("-");
    var d = new Date(Number(p[0]), Number(p[1]) - 1, Number(p[2]));
    return isNaN(d) ? iso : d.toLocaleDateString();
  }

  function initDatePickers(root) {
    root.querySelectorAll("[data-rhx-date-picker]").forEach(function (dp) {
      if (dp._rhxDpInit) return;
      dp._rhxDpInit = true;

      var input = dp.querySelector("[data-rhx-date-display]");
      var trigger = dp.querySelector(".rhx-date-picker__trigger");
      var popup = dp.querySelector(".rhx-date-picker__popup");
      var hidden = dp.querySelector("[data-rhx-date-value]");
      if (!popup || !hidden) return;

      function isOpen() { return !popup.hidden; }

      function open() {
        if (trigger && trigger.hasAttribute("disabled")) return;
        popup.hidden = false;
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        var focusDay = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
        if (focusDay) focusDay.focus();
      }

      function close(focusTrigger) {
        popup.hidden = true;
        if (trigger) trigger.setAttribute("aria-expanded", "false");
        if (focusTrigger && trigger) trigger.focus();
      }

      function commit(iso) {
        hidden.value = iso;
        if (input) input.value = iso ? fmtDisplay(iso) : "";
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
        dp.dispatchEvent(new CustomEvent("rhx:date-picker:change", { bubbles: true, detail: { value: iso } }));
      }

      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close(false) : open(); });
      if (input) input.addEventListener("focus", function () { if (!isOpen()) open(); });

      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { commit(day.getAttribute("data-date")); close(true); return; }
        if (e.target.closest("[data-rhx-cal-today]")) {
          var t = new Date();
          var iso = t.getFullYear() + "-" + String(t.getMonth() + 1).padStart(2, "0") + "-" + String(t.getDate()).padStart(2, "0");
          commit(iso); close(true); return;
        }
        if (e.target.closest("[data-rhx-cal-clear]")) { commit(""); close(true); }
      });

      popup.addEventListener("keydown", function (e) {
        var cur = popup.querySelector(".rhx-calendar__day[tabindex='0']") || document.activeElement;
        if (!cur || !cur.classList || !cur.classList.contains("rhx-calendar__day")) {
          if (e.key === "Escape") { e.preventDefault(); close(true); }
          return;
        }
        var days = Array.prototype.slice.call(popup.querySelectorAll(".rhx-calendar__day"));
        var i = days.indexOf(cur);
        var to = null;
        switch (e.key) {
          case "ArrowRight": to = i + 1; break;
          case "ArrowLeft": to = i - 1; break;
          case "ArrowDown": to = i + 7; break;
          case "ArrowUp": to = i - 7; break;
          case "Home": to = i - (i % 7); break;
          case "End": to = i - (i % 7) + 6; break;
          case "PageUp": e.preventDefault(); clickNav(".rhx-calendar__nav[aria-label='Previous month']"); return;
          case "PageDown": e.preventDefault(); clickNav(".rhx-calendar__nav[aria-label='Next month']"); return;
          case "Enter": case " ":
            e.preventDefault();
            if (!cur.hasAttribute("disabled")) { commit(cur.getAttribute("data-date")); close(true); }
            return;
          case "Escape": e.preventDefault(); close(true); return;
          default: return;
        }
        if (to != null) {
          e.preventDefault();
          if (to < 0 || to >= days.length) { clickNav(to < 0 ? ".rhx-calendar__nav[aria-label='Previous month']" : ".rhx-calendar__nav[aria-label='Next month']"); return; }
          days.forEach(function (d) { d.setAttribute("tabindex", "-1"); });
          days[to].setAttribute("tabindex", "0");
          days[to].focus();
        }
      });

      function clickNav(sel) { var b = popup.querySelector(sel); if (b) b.click(); }

      popup.addEventListener("htmx:afterSwap", function () {
        if (!isOpen()) return;
        var f = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
        if (f) f.focus();
      });

      document.addEventListener("click", function (e) { if (isOpen() && !dp.contains(e.target)) close(false); });
    });
  }

  if (window.RHX) window.RHX.register("date-picker", initDatePickers);
})();
