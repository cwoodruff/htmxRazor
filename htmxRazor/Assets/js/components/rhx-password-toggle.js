/**
 * rhx-password-toggle.js — SANCTIONED JS HOLDOUT (JS* #2, always-on)
 *
 * Why this exists: toggling an <input> between type="password" and type="text"
 * has no HTML/CSS/htmx equivalent — it requires script. This is the entire,
 * deliberately tiny surface for the password-visibility toggle on <rhx-input>.
 * There is no CSS feature on the horizon that retires it, so it is permanent
 * (unlike the temporary platform-gap shims). Opt in with rhx-password-toggle.
 *
 * Delegated: one document-level click listener, no per-element binding/state.
 */
(function () {
  "use strict";

  document.addEventListener("click", function (e) {
    var btn = e.target.closest("[data-rhx-input-toggle]");
    if (!btn) return;

    var control = btn.closest(".rhx-input__control");
    if (!control) return;
    var native = control.querySelector(".rhx-input__native");
    if (!native) return;

    var isPassword = native.type === "password";
    native.type = isPassword ? "text" : "password";

    var showIcon = btn.querySelector(".rhx-input__toggle-show");
    var hideIcon = btn.querySelector(".rhx-input__toggle-hide");
    if (showIcon && hideIcon) {
      showIcon.hidden = isPassword;
      hideIcon.hidden = !isPassword;
    }

    btn.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");
    native.focus();
  });
})();
