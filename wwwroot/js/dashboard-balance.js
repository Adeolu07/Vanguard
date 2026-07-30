document.addEventListener("DOMContentLoaded", () => {
    const toggleBtn = document.getElementById("toggleBalance");
    const balance = document.getElementById("walletBalance");
    if (!toggleBtn || !balance) return;
    let isHidden = false;
    const realBalance = balance.textContent.trim();
    toggleBtn.addEventListener("click", () => {
        if (!isHidden) {
            balance.textContent = " ••••••";
            isHidden = true;
        } else {
            balance.textContent = realBalance;
            isHidden = false;
        }
    });
});