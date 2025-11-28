
// Password toggle for login + register pages
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

// --- BOARD SIZE GROUP ---
const sizeButtons = document.querySelectorAll(".option-btn");
const boardSizeField = document.getElementById("boardSizeField");

sizeButtons.forEach(btn => {
    btn.addEventListener("click", () => {

        sizeButtons.forEach(b => b.classList.remove("selected"));
        btn.classList.add("selected");

        boardSizeField.value = btn.getAttribute("data-value");
    });
});

// --- DIFFICULTY GROUP ---
const diffButtons = document.querySelectorAll(".difficulty-btn");
const difficultyField = document.getElementById("difficultyField");

diffButtons.forEach(btn => {
    btn.addEventListener("click", () => {

        diffButtons.forEach(b => b.classList.remove("selected"));
        btn.classList.add("selected");

        difficultyField.value = btn.getAttribute("data-value");
    });
});

// Select the defaults visually:
document.querySelector(`.option-btn[data-value='@Model.BoardSize']`)?.classList.add("selected");
document.querySelector(`.difficulty-btn[data-value='@Model.DifficultyType']`)?.classList.add("selected");

