/**
 * htmxRazor Toast
 * Handles toast lifecycle: creation from rhx:toast events, auto-dismiss,
 * close buttons, stack management, and hover-to-pause.
 */
(function () {
  "use strict";

  var iconSvgs = {
    neutral: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>',
    brand: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>',
    success: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>',
    warning: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>',
    danger: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>'
  };

  function escapeHtml(str) {
    var div = document.createElement("div");
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
  }

  function getContainer() {
    return document.querySelector("[data-rhx-toast-container]");
  }

  function dismissToast(toast) {
    if (toast._rhxDismissing) return;
    toast._rhxDismissing = true;

    if (toast._rhxTimer) {
      clearTimeout(toast._rhxTimer);
      toast._rhxTimer = null;
    }

    toast.classList.add("rhx-toast--closing");

    function remove() {
      toast.dispatchEvent(new CustomEvent("rhx:toast:close", { bubbles: true }));
      toast.remove();
    }

    toast.addEventListener("animationend", function handler() {
      toast.removeEventListener("animationend", handler);
      remove();
    });

    // Fallback if animation doesn't fire
    setTimeout(function () {
      if (toast.parentNode) remove();
    }, 500);
  }

  function startAutoDismiss(toast, duration) {
    if (duration <= 0) return;
    toast._rhxTimer = setTimeout(function () {
      dismissToast(toast);
    }, duration);
  }

  function enforceMaxToasts(container) {
    var max = parseInt(container.getAttribute("data-rhx-max"), 10) || 5;
    // A dismissed toast stays in the document until its closing
    // animation ends, so count the ones still standing and dismiss the
    // oldest of them once each.
    var standing = Array.prototype.filter.call(
      container.querySelectorAll("[data-rhx-toast]"),
      function (toast) { return !toast._rhxDismissing; });
    for (var i = 0; i < standing.length - max; i++) {
      dismissToast(standing[i]);
    }
  }

  function initToast(toast) {
    if (toast._rhxToastInit) return;
    toast._rhxToastInit = true;

    var container = toast.closest("[data-rhx-toast-container]") || getContainer();
    var containerDuration = container ? parseInt(container.getAttribute("data-rhx-duration"), 10) || 5000 : 5000;

    var toastDuration = toast.hasAttribute("data-rhx-duration")
      ? parseInt(toast.getAttribute("data-rhx-duration"), 10)
      : containerDuration;

    // Close button
    var closeBtn = toast.querySelector(".rhx-toast__close");
    if (closeBtn) {
      closeBtn.addEventListener("click", function () {
        dismissToast(toast);
      });
    }

    // Pause on hover
    toast.addEventListener("mouseenter", function () {
      if (toast._rhxTimer) {
        clearTimeout(toast._rhxTimer);
        toast._rhxTimer = null;
      }
    });

    toast.addEventListener("mouseleave", function () {
      if (!toast._rhxDismissing && toastDuration > 0) {
        startAutoDismiss(toast, toastDuration);
      }
    });

    // Auto-dismiss
    startAutoDismiss(toast, toastDuration);
  }

  function createToast(message, variant, duration) {
    var container = getContainer();
    if (!container) return;

    variant = variant || "neutral";
    var icon = iconSvgs[variant] || iconSvgs.neutral;

    var toast = document.createElement("div");
    toast.className = "rhx-toast rhx-toast--" + variant;
    toast.setAttribute("data-rhx-toast", "");
    if (typeof duration === "number") {
      toast.setAttribute("data-rhx-duration", duration.toString());
    }

    toast.innerHTML =
      '<span class="rhx-toast__icon" aria-hidden="true">' + icon + '</span>' +
      '<div class="rhx-toast__content">' + escapeHtml(message) + '</div>' +
      '<button class="rhx-toast__close" type="button" aria-label="Close">' +
      '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
      '<line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg></button>';

    container.appendChild(toast);
    initToast(toast);
    enforceMaxToasts(container);
  }

  // Listen for rhx:toast events triggered via HX-Trigger headers
  document.addEventListener("rhx:toast", function (e) {
    var detail = e.detail || {};
    createToast(detail.message || "", detail.variant, detail.duration);
  });

  function initToasts(root) {
    var toasts = root.querySelectorAll("[data-rhx-toast]");
    toasts.forEach(initToast);

    var containers = root.querySelectorAll("[data-rhx-toast-container]");
    containers.forEach(enforceMaxToasts);
  }

  if (window.RHX) {
    window.RHX.register("toast", initToasts);
  }
})();
