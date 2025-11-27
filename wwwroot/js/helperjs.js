
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
