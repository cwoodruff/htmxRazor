/* ══════════════════════════════════════════════
   rhx-comparison.js — Before/after image comparison
   ══════════════════════════════════════════════ */
(function () {
    'use strict';

    function init(root) {
        if (root._rhxComparisonInit) return;
        root._rhxComparisonInit = true;

        var afterEl = root.querySelector('.rhx-comparison__after');
        var handle = root.querySelector('.rhx-comparison__handle');

        if (!afterEl || !handle) return;

        function clamp(val, min, max) {
            return Math.max(min, Math.min(max, val));
        }

        function setPosition(percent) {
            percent = clamp(percent, 0, 100);
            afterEl.style.clipPath = 'inset(0 ' + (100 - percent) + '% 0 0)';
            handle.style.left = percent + '%';
            handle.setAttribute('aria-valuenow', Math.round(percent).toString());

            root.dispatchEvent(new CustomEvent('rhx:comparison:change', {
                bubbles: true,
                detail: { position: percent }
            }));
        }

        function getPercent(clientX) {
            var rect = root.getBoundingClientRect();
            var x = clientX - rect.left;
            return (x / rect.width) * 100;
        }

        // ── Mouse/Touch drag ──
        var dragging = false;

        function onPointerDown(e) {
            if (e.button && e.button !== 0) return;
            e.preventDefault();
            dragging = true;
            root.classList.add('rhx-comparison--dragging');

            var clientX = e.touches ? e.touches[0].clientX : e.clientX;
            setPosition(getPercent(clientX));

            document.addEventListener('mousemove', onPointerMove);
            document.addEventListener('mouseup', onPointerUp);
            document.addEventListener('touchmove', onPointerMove, { passive: false });
            document.addEventListener('touchend', onPointerUp);
        }

        function onPointerMove(e) {
            if (!dragging) return;
            e.preventDefault();
            var clientX = e.touches ? e.touches[0].clientX : e.clientX;
            setPosition(getPercent(clientX));
        }

        function onPointerUp() {
            if (!dragging) return;
            dragging = false;
            root.classList.remove('rhx-comparison--dragging');

            document.removeEventListener('mousemove', onPointerMove);
            document.removeEventListener('mouseup', onPointerUp);
            document.removeEventListener('touchmove', onPointerMove);
            document.removeEventListener('touchend', onPointerUp);
        }

        root.addEventListener('mousedown', onPointerDown);
        root.addEventListener('touchstart', onPointerDown, { passive: false });

        // ── Keyboard ──
        handle.addEventListener('keydown', function (e) {
            var current = parseFloat(handle.getAttribute('aria-valuenow')) || 50;
            var step = e.shiftKey ? 10 : 1;
            var newVal = current;

            switch (e.key) {
                case 'ArrowLeft':
                    e.preventDefault();
                    newVal = current - step;
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    newVal = current + step;
                    break;
                case 'Home':
                    e.preventDefault();
                    newVal = 0;
                    break;
                case 'End':
                    e.preventDefault();
                    newVal = 100;
                    break;
                default:
                    return;
            }

            setPosition(newVal);
        });
    }

    // ── Registration ──
    function initWithin(root) {
        (root || document).querySelectorAll('[data-rhx-comparison]').forEach(init);
    }

    if (typeof RHX !== 'undefined' && RHX.register) {
        RHX.register('comparison', initWithin);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { initWithin(document); });
    } else {
        initWithin(document);
    }

    document.addEventListener('htmx:afterSettle', function (e) {
        var el = e.detail.elt;
        if (el && el.querySelectorAll) {
            el.querySelectorAll('[data-rhx-comparison]').forEach(init);
        }
        if (el && el.matches && el.matches('[data-rhx-comparison]')) {
            init(el);
        }
    });
})();
