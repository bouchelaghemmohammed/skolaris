let inactivityTimer;

function resetInactivityTimer() {

    clearTimeout(inactivityTimer);

    inactivityTimer = setTimeout(() => {

        localStorage.removeItem("token");
        localStorage.removeItem("role");
        localStorage.removeItem("nom");
        localStorage.removeItem("userId");

        window.location.href = "/login";

    }, 1800000); // 30 minutes
}

document.addEventListener("mousemove", resetInactivityTimer);
document.addEventListener("keydown", resetInactivityTimer);
document.addEventListener("click", resetInactivityTimer);
document.addEventListener("scroll", resetInactivityTimer);

resetInactivityTimer();
resetInactivityTimer();
