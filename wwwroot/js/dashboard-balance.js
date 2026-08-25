document.addEventListener("DOMContentLoaded", () => {
    const toggleBtn = document.getElementById("toggleBalance");
    const balance = document.getElementById("walletBalance");
    if (!toggleBtn || !balance) return;

    let isHidden = false;
    let realBalance = balance.textContent.trim();

    function render(value) {
        realBalance = value;
        if (!isHidden) balance.textContent = value;
    }

    toggleBtn.addEventListener("click", () => {
        if (!isHidden) {
            balance.textContent = " ••••••";
            isHidden = true;
        } else {
            balance.textContent = realBalance;
            isHidden = false;
        }
    });

    const walletId = document.getElementById("walletId")?.value;
    if (!walletId) return;

    async function loadBalance() {
        try {
            const res = await fetch("/api/wallet/balance", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ customerId: walletId })
            });
            const result = await res.json().catch(() => ({}));

            render(
                result && result.success && result.data
                    ? "₦" + Number(result.data.balance).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                    : "--"
            );
        } catch (err) {
            render("--");
        }
    }

    loadBalance();
});