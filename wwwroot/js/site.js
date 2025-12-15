// js/site.js

$(document).ready(function () {

    // --- GAME TIMER (PERSISTENT ACROSS AJAX UPDATES) --------------------
    let gameTimerInterval = null;

    function startTimer(startTime) {
        if (gameTimerInterval) return; // already running

        const start = new Date(startTime);

        gameTimerInterval = setInterval(() => {
            const now = new Date();
            const elapsed = Math.floor((now - start) / 1000);

            const minutes = Math.floor(elapsed / 60);
            const seconds = elapsed % 60;

            $("#game-timer").text(
                `${minutes}:${seconds.toString().padStart(2, "0")}`
            );

            // --- STOP TIMER & HIDE TIMER UI WHEN GAME OVER ------------------------
            if ($("#game-over-flag").length) {

                // Stop the interval
                clearInterval(gameTimerInterval);
                gameTimerInterval = null;

                // Hide the timer completely
                $(".timer-wrapper").css("display", "none");
            }

        }, 1000);
    }

    // --- INITIAL TIMER START (FIRST PAGE LOAD) -------------------------
    const initialStartTime = $("#game-timer").data("start");
    if (initialStartTime) {
        startTimer(initialStartTime);
    }

    // --- CELL CLICK HANDLER (AJAX) -------------------------------------
    $(document).on("click", ".cell-btn", function (e) {

        e.preventDefault();

        const row = $(this).data("row");
        const col = $(this).data("col");

        $.ajax({
            type: "POST",
            url: "/Game/VisitCellAjax",
            data: { row: row, col: col },

            success: function (updatedHtml) {

                // Replace game UI (state + board)
                $("#game-state-area").html(updatedHtml);

                // Re-read start time ONLY if timer not running
                const newStartTime = $("#game-timer").data("start");
                if (newStartTime) {
                    startTimer(newStartTime);
                }

                // --- AJAX DEMONSTRATION TIMESTAMP ----------------------
                $("#last-update-time").text(
                    new Date().toLocaleTimeString()
                );
            },

            error: function () {
                console.log("Error updating board.");
            }
        });
    });

    // --- RIGHT CLICK (FLAG TOGGLE) ----------------------------------------
    $(document).on("contextmenu", ".cell-btn", function (e) {

        e.preventDefault(); // Stop browser menu

        var row = $(this).data("row");
        var col = $(this).data("col");

        $.ajax({
            type: "POST",
            url: "/Game/ToggleFlagAjax",
            data: { row: row, col: col },

            success: function (updatedHtml) {
                $("#game-state-area").html(updatedHtml);
            },

            error: function () {
                console.log("Error toggling flag.");
            }
        });
    });
});
