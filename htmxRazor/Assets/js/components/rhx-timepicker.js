/**
 * htmxRazor Time Picker
 * A static popup list of times. The visible input opens the list; selecting an option commits a
 * hidden ISO HH:mm value + the display label and closes. Keyboard: Down/Up move, Enter selects,
 * Escape closes, printable keys type-ahead by label. Positioning reuses rhx-position.js.
 */
(function () {
  "use strict";

  var OPT = ".rhx-time-picker__option:not([disabled])";

  function initTimePickers(root) {
    root.querySelectorAll("[data-rhx-time-picker]").forEach(function (tp) {
      if (tp._rhxTpInit) return;
      tp._rhxTpInit = true;

      var input = tp.querySelector("[data-rhx-time-display]");
      var trigger = tp.querySelector(".rhx-time-picker__trigger");
      var listbox = tp.querySelector(".rhx-time-picker__listbox");
      var hidden = tp.querySelector("[data-rhx-time-value]");
      if (!input || !listbox || !hidden) return;

      // Guard: prevents the focus handler from re-opening immediately after commit()
      // calls close() + input.focus(). Set to true in commit(), cleared on next tick.
      var suppressFocusOpen = false;

      function isOpen() { return !listbox.hidden; }

      function open() {
        if (input.hasAttribute("disabled") || input.hasAttribute("readonly")) return;
        listbox.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, listbox, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
        var sel = listbox.querySelector(".rhx-time-picker__option--selected") || listbox.querySelector(OPT);
        focusOption(sel, true);
      }

      function close() {
        listbox.hidden = true;
        input.setAttribute("aria-expanded", "false");
        clearFocused();
      }

      function options() { return Array.prototype.slice.call(listbox.querySelectorAll(OPT)); }

      function clearFocused() {
        var f = listbox.querySelector("[data-rhx-focused]");
        if (f) f.removeAttribute("data-rhx-focused");
        input.removeAttribute("aria-activedescendant");
      }

      function focusOption(opt, scroll) {
        if (!opt) return;
        clearFocused();
        if (!opt.id) opt.id = listbox.id + "-o" + options().indexOf(opt);
        opt.setAttribute("data-rhx-focused", "");
        input.setAttribute("aria-activedescendant", opt.id);
        if (scroll) opt.scrollIntoView({ block: "nearest" });
      }

      function move(delta) {
        var opts = options();
        if (!opts.length) return;
        var cur = listbox.querySelector("[data-rhx-focused]");
        var i = cur ? opts.indexOf(cur) : -1;
        var next = Math.max(0, Math.min(opts.length - 1, i + delta));
        focusOption(opts[next], true);
      }

      function commit(opt) {
        if (!opt) return;
        var prev = listbox.querySelector(".rhx-time-picker__option--selected");
        if (prev) {
          prev.classList.remove("rhx-time-picker__option--selected");
          prev.removeAttribute("aria-selected");
        }
        opt.classList.add("rhx-time-picker__option--selected");
        opt.setAttribute("aria-selected", "true");
        input.value = opt.textContent.trim();
        hidden.value = opt.getAttribute("data-time") || "";
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
        tp.dispatchEvent(new CustomEvent("rhx:time-picker:change", { bubbles: true, detail: { value: hidden.value } }));
        // Suppress the focus handler for one tick so that close() + input.focus()
        // below does not immediately re-open the listbox.
        suppressFocusOpen = true;
        close();
        input.focus();
        // Use setTimeout 0 rather than a microtask so the flag outlives the focus
        // event that fires synchronously inside input.focus().
        setTimeout(function () { suppressFocusOpen = false; }, 0);
      }

      // Open on input focus — but not immediately after a commit (suppressFocusOpen guard).
      input.addEventListener("focus", function () {
        if (!suppressFocusOpen && !isOpen()) open();
      });

      // Trigger click: toggle. Calls input.focus() which might otherwise open the
      // listbox on the close branch; the trigger click handler does its own open()
      // call explicitly, so input.focus() is deferred to after open() here.
      if (trigger) {
        trigger.addEventListener("click", function () {
          if (isOpen()) {
            close();
          } else {
            open();
            input.focus();
          }
        });
      }

      listbox.addEventListener("click", function (e) {
        var opt = e.target.closest(OPT);
        if (opt && listbox.contains(opt)) commit(opt);
      });

      input.addEventListener("keydown", function (e) {
        var opts, f;
        switch (e.key) {
          case "ArrowDown":
            e.preventDefault();
            if (!isOpen()) open(); else move(1);
            break;
          case "ArrowUp":
            e.preventDefault();
            if (!isOpen()) open(); else move(-1);
            break;
          case "Home":
            if (isOpen()) { e.preventDefault(); focusOption(options()[0], true); }
            break;
          case "End":
            if (isOpen()) { e.preventDefault(); opts = options(); focusOption(opts[opts.length - 1], true); }
            break;
          case "Enter":
            if (isOpen()) {
              e.preventDefault();
              f = listbox.querySelector("[data-rhx-focused]");
              if (f) commit(f);
            }
            break;
          case "Escape":
            if (isOpen()) { e.preventDefault(); close(); }
            break;
          case "Tab":
            if (isOpen()) close();
            break;
          default:
            if (e.key.length === 1 && /\S/.test(e.key)) {
              if (!isOpen()) open();
              var q = e.key.toLowerCase();
              var match = options().filter(function (o) {
                return o.textContent.trim().toLowerCase().indexOf(q) === 0;
              })[0];
              if (match) focusOption(match, true);
            }
        }
      });

      document.addEventListener("click", function (e) {
        if (isOpen() && !tp.contains(e.target)) close();
      });
    });
  }

  if (window.RHX) window.RHX.register("time-picker", initTimePickers);
})();
