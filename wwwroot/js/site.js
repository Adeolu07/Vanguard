document.addEventListener('DOMContentLoaded', function () {
    const walletId = document.getElementById('walletId')?.value;
    const balance = document.getElementById('walletBalance');

    if (!walletId) {
        if (balance)
            balance.textContent = 'Wallet not linked';
        return;
    }

    async function postJson(url, body) {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        let json = {};
        try { json = await res.json(); } catch (e) { /* non-JSON body */ }
        return json; // { success, data, error }
    }

    function money(value) {
        return '₦' + Number(value || 0).toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // ---- Balance refresh ----
    async function loadBalance() {
        try {
            const result = await postJson('/api/wallet/balance', { customerId: walletId });
            if (balance) {
                balance.textContent = result && result.success && result.data
                    ? money(result.data.balance)
                    : '--';
            }
        } catch (err) {
            if (balance) balance.textContent = '--';
        }
    }

    // ---- Transaction receipt modal ----
    function initReceiptModal() {
        let overlay = document.getElementById('receiptOverlay');
        let closeBtn = document.getElementById('receiptClose');
        let printBtn = document.getElementById('receiptPrintBtn');
        let rows = document.querySelectorAll('.txn-row');

        let creditUpIcon = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="19" x2="12" y2="5"></line><polyline points="5 12 12 5 19 12"></polyline></svg>';
        let debitDownIcon = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"></line><polyline points="19 12 12 19 5 12"></polyline></svg>';

        function openReceipt(data) {
            let isCredit = (data.type || '').toLowerCase() === 'credit';
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

            document.getElementById('printAmount').textContent = sign + '₦' + data.amount;
            document.getElementById('printTypeLabel').textContent = data.type + ' Transaction';
            document.getElementById('printDescription').textContent = data.description || '—';
            document.getElementById('printTransactionId').textContent = data.transactionId || '—';
            document.getElementById('printSessionId').textContent = data.sessionId || '—';
            document.getElementById('printSessionRow').style.display = data.sessionId ? '' : 'none';

            overlay.classList.add('active');
            document.body.style.overflow = 'hidden';
        }

        function closeModal() {
            overlay.classList.remove('active');
            document.body.style.overflow = '';
        }

        rows.forEach(row => {
            row.style.cursor = 'pointer';
            row.addEventListener('click', function () {
                const transactionId = row.getAttribute('data-transaction-id');
                const type = row.getAttribute('data-type') || '';
                const amount = row.getAttribute('data-amount') || '';
                const description = row.getAttribute('data-description') || '';
                const sessionId = row.getAttribute('data-session-id') || '';

                openReceipt({ type, amount, description, transactionId, sessionId });

                postJson('/api/wallet/transaction', { transactionId: transactionId })
                    .then(result => {
                        if (result && result.success && result.data) {
                            const d = result.data;
                            openReceipt({
                                type: d.type || type,
                                amount: d.amount != null ? Number(d.amount).toLocaleString() : amount,
                                description: d.description || description,
                                transactionId: d.transactionId || transactionId,
                                sessionId: d.sessionId || sessionId
                            });
                        }
                    })
                    .catch(function () { /* keep placeholder data */ });
            });
        });

        closeBtn.addEventListener('click', closeModal);
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay)
                closeModal();
        });

        printBtn.addEventListener('click', function () {
            window.print();
        });
    }

    // ---- Fund wallet via bank transfer modal ----
    function initFundWalletModal() {
        const fundBtn = document.getElementById('fundButton');
        const overlay = document.getElementById('fundModalOverlay');
        const closeBtn = document.getElementById('fundModalClose');
        const copyBtn = document.getElementById('copyAccountBtn');
        const msgEl = document.getElementById('fundMessage');
        if (!fundBtn || !overlay)
            return;

        function closeModal() {
            overlay.classList.remove('active');
            document.body.style.overflow = '';
        }

        closeBtn?.addEventListener('click', closeModal);
        overlay.addEventListener('click', e => {
            if (e.target === overlay) closeModal();
        });

        copyBtn?.addEventListener('click', () => {
            const value = document.getElementById('modalAccountNumber').textContent;
            if (navigator.clipboard?.writeText) {
                navigator.clipboard.writeText(value);
            } else {
                const ta = document.createElement('textarea');
                ta.value = value;
                document.body.appendChild(ta);
                ta.select();
                document.execCommand('copy');
                document.body.removeChild(ta);
            }
            copyBtn.textContent = 'Copied!';
            setTimeout(() => (copyBtn.textContent = 'Copy'), 1500);
        });

        fundBtn.addEventListener('click', async () => {
            msgEl.textContent = '';
            msgEl.className = 'fund-message';
            fundBtn.disabled = true;
            fundBtn.textContent = 'Loading…';

            try {
                const result = await postJson('/api/wallet/nameenquiry', { customerId: walletId });
                

                if (!result || !result.success || !result.data) {
                    msgEl.textContent = (result && result.error) || 'Unable load account details.';
                    msgEl.className = 'fund-message error';
                    return;
                }

                document.getElementById('modalAccountNumber').textContent = result.data.accountNumber || '—';
                document.getElementById('modalBankName').textContent = result.data.bankName || '—';
                document.getElementById('modalAccountName').textContent = result.data.accountName || '—';
                overlay.classList.add('active');
                document.body.style.overflow = 'hidden';
            } catch (err) {
                msgEl.textContent = 'Network error. Please try again.';
                msgEl.className = 'fund-message error';
            } finally {
                fundBtn.disabled = false;
                fundBtn.textContent = 'Fund Wallet';
            }
        });
    }

    // ---- Boot ----
    loadBalance().then(r => console.log("done"));
    initReceiptModal();
    initFundWalletModal();
});