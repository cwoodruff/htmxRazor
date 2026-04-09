/**
 * htmxRazor Kanban Board
 * Native HTML Drag and Drop with htmx integration for server-side persistence.
 * Supports mouse drag, keyboard navigation (Enter/Space to grab, Arrow keys, Escape),
 * and touch devices.
 */
(function () {
  "use strict";

  function initKanban(root) {
    var boards = root.querySelectorAll("[data-rhx-kanban]");
    boards.forEach(function (board) {
      if (board._rhxKanbanInit) return;
      board._rhxKanbanInit = true;

      var draggedCard = null;
      var sourceColumn = null;

      // ── Drag events on cards ──
      board.addEventListener("dragstart", function (e) {
        var card = e.target.closest("[data-rhx-kanban-card]");
        if (!card) return;

        draggedCard = card;
        sourceColumn = card.closest("[data-rhx-kanban-column]");

        card.classList.add("rhx-kanban-card--dragging");
        e.dataTransfer.effectAllowed = "move";
        e.dataTransfer.setData("text/plain", card.getAttribute("data-rhx-card-id"));
      });

      board.addEventListener("dragend", function (e) {
        var card = e.target.closest("[data-rhx-kanban-card]");
        if (card) {
          card.classList.remove("rhx-kanban-card--dragging");
        }
        draggedCard = null;
        sourceColumn = null;

        // Remove all drag-over states
        board.querySelectorAll(".rhx-kanban-column--drag-over").forEach(function (col) {
          col.classList.remove("rhx-kanban-column--drag-over");
        });
      });

      // ── Drop zone events on columns ──
      board.addEventListener("dragover", function (e) {
        var dropZone = e.target.closest("[data-rhx-drop-zone]");
        var column = e.target.closest("[data-rhx-kanban-column]");
        if (!dropZone || !column) return;
        if (column.hasAttribute("data-rhx-no-drop")) return;

        e.preventDefault();
        e.dataTransfer.dropEffect = "move";
        column.classList.add("rhx-kanban-column--drag-over");
      });

      board.addEventListener("dragleave", function (e) {
        var column = e.target.closest("[data-rhx-kanban-column]");
        if (!column) return;

        // Only remove if actually leaving the column
        var relatedColumn = e.relatedTarget ? e.relatedTarget.closest("[data-rhx-kanban-column]") : null;
        if (relatedColumn !== column) {
          column.classList.remove("rhx-kanban-column--drag-over");
        }
      });

      board.addEventListener("drop", function (e) {
        e.preventDefault();
        var dropZone = e.target.closest("[data-rhx-drop-zone]");
        var targetColumn = e.target.closest("[data-rhx-kanban-column]");
        if (!dropZone || !targetColumn || !draggedCard) return;
        if (targetColumn.hasAttribute("data-rhx-no-drop")) return;

        targetColumn.classList.remove("rhx-kanban-column--drag-over");

        var cardId = draggedCard.getAttribute("data-rhx-card-id");
        var sourceColumnId = sourceColumn ? sourceColumn.getAttribute("data-rhx-column-id") : "";
        var targetColumnId = targetColumn.getAttribute("data-rhx-column-id");

        // Determine position within column
        var cards = Array.from(dropZone.querySelectorAll("[data-rhx-kanban-card]:not(.rhx-kanban-card--dragging)"));
        var position = cards.length; // Default: append at end

        // Find insertion point based on mouse position
        for (var i = 0; i < cards.length; i++) {
          var rect = cards[i].getBoundingClientRect();
          if (e.clientY < rect.top + rect.height / 2) {
            position = i;
            break;
          }
        }

        // Optimistic DOM move
        if (position < cards.length) {
          dropZone.insertBefore(draggedCard, cards[position]);
        } else {
          dropZone.appendChild(draggedCard);
        }

        // Check WIP limits
        checkWipLimits(board);

        // Fire htmx POST
        fireCardMove(board, draggedCard, cardId, sourceColumnId, targetColumnId, position);
      });

      // ── Shared: fire htmx POST for a card move ──
      function fireCardMove(board, card, cardId, sourceColumnId, targetColumnId, position) {
        var postUrl = card.getAttribute("hx-post") ||
                      card.getAttribute("data-hx-post");

        if (postUrl && typeof htmx !== "undefined") {
          var hxTarget = card.getAttribute("hx-target") ||
                         card.getAttribute("data-hx-target");
          var hxSwap = card.getAttribute("hx-swap") ||
                       card.getAttribute("data-hx-swap") || "innerHTML";

          var target = hxTarget ? document.querySelector(hxTarget) : board;

          htmx.ajax("POST", postUrl, {
            target: target,
            swap: hxSwap,
            values: {
              cardId: cardId,
              sourceColumn: sourceColumnId,
              targetColumn: targetColumnId,
              position: position
            }
          });
        }

        board.dispatchEvent(new CustomEvent("rhx:kanban:drop", {
          bubbles: true,
          detail: {
            cardId: cardId,
            sourceColumn: sourceColumnId,
            targetColumn: targetColumnId,
            position: position
          }
        }));
      }

      // ── Keyboard navigation ──
      board.addEventListener("keydown", function (e) {
        var card = e.target.closest("[data-rhx-kanban-card]");
        if (!card) return;

        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          if (card.classList.contains("rhx-kanban-card--grabbed")) {
            // Drop the card — fire the POST to persist
            card.classList.remove("rhx-kanban-card--grabbed");
            card.setAttribute("aria-grabbed", "false");

            var column = card.closest("[data-rhx-kanban-column]");
            var dropZone = column ? column.querySelector("[data-rhx-drop-zone]") : null;
            if (column && dropZone) {
              var allCards = Array.from(dropZone.querySelectorAll("[data-rhx-kanban-card]"));
              var position = allCards.indexOf(card);
              var cardId = card.getAttribute("data-rhx-card-id");
              var targetColumnId = column.getAttribute("data-rhx-column-id");
              // sourceColumn is unknown after keyboard moves, send targetColumn as source
              fireCardMove(board, card, cardId, targetColumnId, targetColumnId, position >= 0 ? position : 0);
            }
          } else {
            // Grab the card
            card.classList.add("rhx-kanban-card--grabbed");
            card.setAttribute("aria-grabbed", "true");
          }
        } else if (e.key === "Escape") {
          card.classList.remove("rhx-kanban-card--grabbed");
          card.setAttribute("aria-grabbed", "false");
        } else if (card.classList.contains("rhx-kanban-card--grabbed")) {
          var column = card.closest("[data-rhx-kanban-column]");
          var columns = Array.from(board.querySelectorAll("[data-rhx-kanban-column]"));
          var colIdx = columns.indexOf(column);

          if (e.key === "ArrowLeft" && colIdx > 0) {
            e.preventDefault();
            var prevCol = columns[colIdx - 1];
            var prevBody = prevCol.querySelector("[data-rhx-drop-zone]");
            if (prevBody && !prevCol.hasAttribute("data-rhx-no-drop")) {
              prevBody.appendChild(card);
              card.focus();
              checkWipLimits(board);
            }
          } else if (e.key === "ArrowRight" && colIdx < columns.length - 1) {
            e.preventDefault();
            var nextCol = columns[colIdx + 1];
            var nextBody = nextCol.querySelector("[data-rhx-drop-zone]");
            if (nextBody && !nextCol.hasAttribute("data-rhx-no-drop")) {
              nextBody.appendChild(card);
              card.focus();
              checkWipLimits(board);
            }
          } else if (e.key === "ArrowUp") {
            e.preventDefault();
            var prev = card.previousElementSibling;
            if (prev && prev.hasAttribute("data-rhx-kanban-card")) {
              card.parentNode.insertBefore(card, prev);
              card.focus();
            }
          } else if (e.key === "ArrowDown") {
            e.preventDefault();
            var next = card.nextElementSibling;
            if (next && next.hasAttribute("data-rhx-kanban-card")) {
              card.parentNode.insertBefore(next, card);
              card.focus();
            }
          }
        }
      });

      // ── WIP limit checking ──
      function checkWipLimits(board) {
        board.querySelectorAll("[data-rhx-kanban-column]").forEach(function (col) {
          var max = col.getAttribute("data-rhx-max-cards");
          if (!max) return;

          var count = col.querySelectorAll("[data-rhx-kanban-card]").length;
          col.classList.toggle("rhx-kanban-column--wip-exceeded", count >= parseInt(max, 10));
        });
      }

      // Initial WIP check
      checkWipLimits(board);
    });
  }

  if (window.RHX) {
    window.RHX.register("kanban", initKanban);
  }
})();
