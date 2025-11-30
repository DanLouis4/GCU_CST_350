/***************************************************************
 *  PASSWORD VISIBILITY TOGGLE (Login + Register)
 ***************************************************************/
document.addEventListener("DOMContentLoaded", function () {
    const passField = document.getElementById("passwordField");
    const toggleBtn = document.getElementById("togglePass");

    if (passField && toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            const isPassword = passField.type === "password";
            passField.type = isPassword ? "text" : "password";
            toggleBtn.innerText = isPassword ? "Hide" : "Show";
        });
    }
});


/***************************************************************
 *  START GAME PAGE — BOARD SIZE SELECTION GROUP
 ***************************************************************/
const sizeButtons = document.querySelectorAll(".option-btn");
const boardSizeField = document.getElementById("boardSizeField");

if (sizeButtons && boardSizeField) {
    sizeButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            sizeButtons.forEach(b => b.classList.remove("selected"));
            btn.classList.add("selected");
            boardSizeField.value = btn.getAttribute("data-value");
        });
    });
}


/***************************************************************
 *  START GAME PAGE — DIFFICULTY SELECTION GROUP
 ***************************************************************/
const diffButtons = document.querySelectorAll(".difficulty-btn");
const difficultyField = document.getElementById("difficultyField");

if (diffButtons && difficultyField) {
    diffButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            diffButtons.forEach(b => b.classList.remove("selected"));
            btn.classList.add("selected");
            difficultyField.value = btn.getAttribute("data-value");
        });
    });
}


/***************************************************************
 *  DEFAULT SELECTION APPLICATION (StartGame Page)
 ***************************************************************/
function applyDefaultSelections() {
    const defaultSize = boardSizeField?.value;
    const defaultDifficulty = difficultyField?.value;

    if (defaultSize) {
        document
            .querySelector(`.option-btn[data-value='${defaultSize}']`)
            ?.classList.add("selected");
    }

    if (defaultDifficulty) {
        document
            .querySelector(`.difficulty-btn[data-value='${defaultDifficulty}']`)
            ?.classList.add("selected");
    }
}

document.addEventListener("DOMContentLoaded", applyDefaultSelections);


/***************************************************************
 *  MINESWEEPER BOARD — TIMER SYSTEM
 *  Automatically activates ONLY on the game board page.
 ***************************************************************/
function initializeBoardTimer() {
    const timerDisplay = document.getElementById("timer-display");
    const startTimeAttr = document.getElementById("timer-display")?.getAttribute("data-start");

    // Exit if not on the board page
    if (!timerDisplay || !startTimeAttr) return;

    const startTime = new Date(startTimeAttr);

    function updateTimer() {
        const now = new Date();
        let elapsed = Math.floor((now - startTime) / 1000); // total seconds

        const minutes = Math.floor(elapsed / 60);
        const seconds = elapsed % 60;

        timerDisplay.innerText =
            minutes + ":" + (seconds < 10 ? "0" + seconds : seconds);
    }

    // Run and keep running
    updateTimer();
    setInterval(updateTimer, 1000);
}

document.addEventListener("DOMContentLoaded", initializeBoardTimer);
