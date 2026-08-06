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

function omo()
{
    let overlay = document.getElementById('receiptOverlay');
    let closeBtn = document.getElementById('receiptClose');
    let printBtn = document.getElementById('receiptPrintBtn');
    let rows = document.querySelectorAll('.txn-row');

    let creditUpIcon = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="19" x2="12" y2="5"></line><polyline points="5 12 12 5 19 12"></polyline></svg>';
    let debitDownIcon = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"></line><polyline points="19 12 12 19 5 12"></polyline></svg>';

    function openReceipt(data) {
        let isCredit = data.type.toLowerCase() === 'credit';
        let sign = isCredit ? '+' : '–';

        let statusIcon = document.getElementById('receiptStatusIcon');
        statusIcon.className = 'receipt-status-icon ' + (isCredit ? 'credit' : 'debit');
        statusIcon.innerHTML = isCredit ? creditUpIcon : debitDownIcon;

        let amountEl = document.getElementById('receiptAmount');
        amountEl.className = isCredit ? 'credit' : 'debit';
        amountEl.textContent = sign + '₦' + data.amount;

        document.getElementById('receiptTypeLabel').textContent = data.type + ' Transaction';
        document.getElementById('receiptDescription').textContent = data.description || '—';
        document.getElementById('receiptTransactionId').textContent = data.transactionId || '—';

        let sessionRow = document.getElementById('receiptSessionRow');
        if (data.sessionId) {
            sessionRow.style.display = 'flex';
            document.getElementById('receiptSessionId').textContent = data.sessionId;
        } else {
            sessionRow.style.display = 'none';
        }

        // Sync hidden printable receipt
        document.getElementById('printAmount').textContent = sign + '₦' + data.amount;
        document.getElementById('printAmount').style.color = isCredit ? '#10b981' : '#ef4444';
        document.getElementById('printTypeLabel').textContent = data.type + ' Transaction';
        document.getElementById('printDescription').textContent = data.description || '—';
        document.getElementById('printTransactionId').textContent = data.transactionId || '—';
        var printSessionRow = document.getElementById('printSessionRow');
        if (data.sessionId) {
            printSessionRow.style.display = '';
            document.getElementById('printSessionId').textContent = data.sessionId;
        } else {
            printSessionRow.style.display = 'none';
        }

        overlay.classList.add('open');
    }

    function closeReceipt() {
        overlay.classList.remove('open');
    }

    rows.forEach(function (row) {
        row.addEventListener('click', function () {
            openReceipt({
                type: row.getAttribute('data-type'),
                amount: row.getAttribute('data-amount'),
                description: row.getAttribute('data-description'),
                transactionId: row.getAttribute('data-transaction-id'),
                sessionId: row.getAttribute('data-session-id')
            });
        });
    });

    closeBtn.addEventListener('click', closeReceipt);
    overlay.addEventListener('click', function (e) {
        if (e.target === overlay) closeReceipt();
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeReceipt();
    });

    printBtn.addEventListener('click', function () {
        window.print();
    });
}