document.addEventListener('DOMContentLoaded', function () {
    const walletId = document.getElementById('walletId')?.value;
    const balanceEl = document.getElementById('walletBalance');
    const fundButton = document.getElementById('fundButton');
    const fundAmount = document.getElementById('fundAmount');
    const fundDesc = document.getElementById('fundDescription');
    const fundMessage = document.getElementById('fundMessage');

    if (!walletId) {
        if (balanceEl) balanceEl.textContent = 'Wallet not linked';
        return;
    }
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
    async function loadBalance() {
        try {
            const res = await fetch('/api/wallet/balance', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ customerId: walletId })
            });
            const data = await res.json();
            if (balanceEl) {
                balanceEl.textContent = data.responseHeader?.responseCode === '00'
                    ? `₦${data.balance.toLocaleString()}`
                    : '--';
            }
        } catch (err) {
            if (balanceEl) balanceEl.textContent = 'Error';
        }
    }

    if (fundButton) {
        fundButton.addEventListener('click', async () => {
            const amount = parseFloat(fundAmount.value);
            if (isNaN(amount) || amount <= 0) {
                fundMessage.innerHTML = '<span class="error">Enter a valid amount</span>';
                return;
            }
            const desc = fundDesc.value;
            fundButton.disabled = true;
            fundMessage.innerHTML = 'Funding...';
            try {
                const traceId = crypto.randomUUID().replace(/-/g, '');
                const res = await fetch('/api/wallet/credit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        customerId: walletId,
                        amount: amount,
                        description: desc || 'Wallet funding',
                        traceId: traceId
                    })
                });
                const data = await res.json();
                if (data.responseHeader?.responseCode === '00') {
                    await loadBalance();
                    const newBalance = document.getElementById('walletBalance').textContent;
                    fundMessage.innerHTML = `<span class="success">✅ Wallet funded – New balance: ${newBalance}</span>`;
                } else {
                    fundMessage.innerHTML = '<span class="error">❌ Funding failed</span>';
                }
            } catch (err) {
                fundMessage.innerHTML = '<span class="error">Network error</span>';
            } finally {
                fundButton.disabled = false;
            }
        });
    }

    loadBalance();
});