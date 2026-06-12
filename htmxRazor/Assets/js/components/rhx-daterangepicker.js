/**
 * htmxRazor Date Range Picker
 * Two side-by-side months. First day click sets the range start; the second sets the end (swapped
 * if earlier). While picking, hovering a day shows a live in-range preview. Presets set both dates.
 * Range highlighting is painted entirely client-side onto the .rhx-calendar__day cells (on init,
 * select, hover, and after each htmx month swap). Commits two hidden ISO yyyy-MM-dd inputs.
 * Display labels come from the server-rendered data-display attribute on each visible day cell;
 * dates outside the two visible months (e.g. some presets) fall back to the browser's
 * toLocaleDateString(), consistent with the date picker.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";

  function iso(d) {
    return d.getFullYear() + "-" + String(d.getMonth() + 1).padStart(2, "0") + "-" + String(d.getDate()).padStart(2, "0");
  }
  function parse(s) { var p = (s || "").split("-"); return p.length === 3 ? new Date(Number(p[0]), Number(p[1]) - 1, Number(p[2])) : null; }

  function initRangePickers(root) {
    root.querySelectorAll("[data-rhx-date-range-picker]").forEach(function (rp) {
      if (rp._rhxRpInit) return;
      rp._rhxRpInit = true;

      var input = rp.querySelector("[data-rhx-range-display]");
      var trigger = rp.querySelector(".rhx-date-range-picker__trigger");
      var popup = rp.querySelector(".rhx-date-range-picker__popup");
      var hiddenStart = rp.querySelector("[data-rhx-range-start]");
      var hiddenEnd = rp.querySelector("[data-rhx-range-end]");
      if (!input || !popup || !hiddenStart || !hiddenEnd) return;

      var startIso = rp.getAttribute("data-range-start") || "";
      var endIso = rp.getAttribute("data-range-end") || "";
      var selecting = false;

      function isOpen() { return !popup.hidden; }

      function open() {
        if (input.hasAttribute("disabled") || input.hasAttribute("readonly")) return;
        popup.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, popup, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
        paint();
      }

      function close() {
        // If the popup is dismissed mid-selection (start picked, no end yet), abandon the partial range.
        if (selecting && !endIso) startIso = "";
        selecting = false;
        popup.hidden = true;
        input.setAttribute("aria-expanded", "false");
        if (trigger) trigger.setAttribute("aria-expanded", "false");
        display();
      }

      function paint(hoverIso) {
        var lo, hi;
        if (selecting && startIso && hoverIso) {
          lo = startIso < hoverIso ? startIso : hoverIso;
          hi = startIso < hoverIso ? hoverIso : startIso;
        } else if (startIso && endIso) {
          lo = startIso < endIso ? startIso : endIso;
          hi = startIso < endIso ? endIso : startIso;
        } else {
          lo = startIso;
          hi = startIso;
        }
        popup.querySelectorAll(".rhx-calendar__day").forEach(function (c) {
          c.classList.remove("rhx-calendar__day--in-range", "rhx-calendar__day--range-start", "rhx-calendar__day--range-end");
          if (c.hasAttribute("disabled")) return;
          var d = c.getAttribute("data-date");
          if (!d || !lo) return;
          if (d === lo) c.classList.add("rhx-calendar__day--range-start");
          if (hi && d === hi) c.classList.add("rhx-calendar__day--range-end");
          if (hi && d > lo && d < hi) c.classList.add("rhx-calendar__day--in-range");
        });
      }

      function commit() {
        hiddenStart.value = startIso;
        hiddenEnd.value = endIso;
        hiddenStart.dispatchEvent(new Event("change", { bubbles: true }));
        hiddenEnd.dispatchEvent(new Event("change", { bubbles: true }));
        rp.dispatchEvent(new CustomEvent("rhx:date-range-picker:change", { bubbles: true, detail: { start: startIso, end: endIso } }));
      }

      function dispOf(isoStr) {
        var cell = popup.querySelector('.rhx-calendar__day[data-date="' + isoStr + '"]');
        if (cell && cell.getAttribute("data-display")) return cell.getAttribute("data-display");
        var d = parse(isoStr);
        return d ? d.toLocaleDateString() : isoStr;
      }

      function display() {
        if (startIso && endIso) input.value = dispOf(startIso) + " – " + dispOf(endIso);
        else if (startIso) input.value = dispOf(startIso) + " – …";
        else input.value = "";
      }

      function pickDay(cell) {
        var d = cell.getAttribute("data-date");
        if (!startIso || (startIso && endIso)) {
          // Begin a new range (either nothing selected, or both already set)
          startIso = d; endIso = ""; selecting = true;
        } else {
          // Complete the range — swap if end is before start
          if (d < startIso) { endIso = startIso; startIso = d; } else { endIso = d; }
          selecting = false;
        }
        paint();
        display();
        if (!selecting) { commit(); close(); }
      }

      function applyPreset(key) {
        var t = new Date(); t.setHours(0, 0, 0, 0);
        var s = new Date(t), e = new Date(t);
        switch (key) {
          case "today": break;
          case "yesterday": s.setDate(s.getDate() - 1); e.setDate(e.getDate() - 1); break;
          case "last7": s.setDate(s.getDate() - 6); break;
          case "last30": s.setDate(s.getDate() - 29); break;
          case "thismonth": s = new Date(t.getFullYear(), t.getMonth(), 1); e = new Date(t.getFullYear(), t.getMonth() + 1, 0); break;
          case "lastmonth": s = new Date(t.getFullYear(), t.getMonth() - 1, 1); e = new Date(t.getFullYear(), t.getMonth(), 0); break;
          default: return;
        }
        startIso = iso(s); endIso = iso(e); selecting = false;
        paint(); display(); commit(); close();
      }

      // --- Event wiring ---

      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close() : open(); });
      input.addEventListener("focus", function () { if (!isOpen()) open(); });

      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { pickDay(day); return; }
        var preset = e.target.closest("[data-range-preset]");
        if (preset) applyPreset(preset.getAttribute("data-range-preset"));
      });

      popup.addEventListener("mouseover", function (e) {
        if (!selecting) return;
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) paint(day.getAttribute("data-date"));
      });
      popup.addEventListener("mouseleave", function () { if (selecting) paint(); });

      popup.addEventListener("htmx:afterSwap", function () { paint(); });

      document.addEventListener("click", function (e) { if (isOpen() && !rp.contains(e.target)) close(); });

      // Paint initial state if seeded values exist
      if (startIso || endIso) display();
    });
  }

  if (window.RHX) window.RHX.register("date-range-picker", initRangePickers);
})();
