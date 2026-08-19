document.addEventListener('DOMContentLoaded', function () {
    const walletId = document.getElementById('walletId')?.value;
    const balance = document.getElementById('walletBalance');

    if (!walletId) {
        if (balance) 
            balance.textContent = 'Wallet not linked';
        return;
    }

    const pick = (obj, ...keys) => {
        for (const k of keys) if (obj && obj[k] != null && obj[k] !== '') return obj[k];
        return null;
    };

    // ---- Balance refresh ----
    async function loadBalance(){
        try {
            const res = await fetch('/api/wallet/balance', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ customerId: walletId })
            });
            const data = await res.json();
            if (balance) {
                const code = pick(data?.responseHeader ?? data?.ResponseHeader, 'responseCode', 'ResponseCode');
                const bal = pick(data, 'balance', 'Balance');
                balance.textContent = code === '00' && bal != null
                    ? '₦' + Number(bal).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                    : '--';
            }
        } catch (err) {
            if (balance) balance.textContent = 'Error';
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
            document.getElementById('printTypeLabel').textContent = data.type + ' Transaction';
            document.getElementById('printDescription').textContent = data.description || '—';
            document.getElementById('printTransactionId').textContent = data.transactionId || '—';
            document.getElementById('printSessionId').textContent = data.sessionId || '—';
            if (data.sessionId) {
                document.getElementById('printSessionRow').style.display = '';
            } else {
                document.getElementById('printSessionRow').style.display = 'none';
            }

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

                // Show placeholder data immediately
                openReceipt({ type, amount, description, transactionId, sessionId });

                // Fetch full details and update
                fetch('/api/wallet/transaction', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ transactionId: transactionId })
                })
                    .then(res => res.json())
                    .then(res => {
                        if (res && res.transactionDetails) {
                            const details = res.transactionDetails;
                            openReceipt({
                                type: details.tranType || type,
                                amount: details.amount ? details.amount.toLocaleString() : amount,
                                description: details.description || description,
                                transactionId: details.transactionId || transactionId,
                                sessionId: details.sessionId || sessionId
                            });
                        }
                    })
                    .catch(err => console.warn('API fetch failed', err));
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

    // ---- Fund wallet ----
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

        // Bind close handlers ONCE (these were missing)
        closeBtn?.addEventListener('click', closeModal);
        overlay.addEventListener('click', e => {
            if (e.target === overlay) closeModal();
        });

        // Bind copy handler ONCE (was incorrectly inside the finally below)
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
                const res = await fetch('/api/wallet/nameenquiry', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ customerId: walletId })
                });
                const data = await res.json().catch(() => ({}));

                if (!res.ok) {
                    const header = data.responseHeader ?? data.ResponseHeader;
                    msgEl.textContent = header?.responseMessage || 'Unable to load account details.';
                    msgEl.className = 'fund-message error';
                    return;
                }

                const header = data.responseHeader ?? data.ResponseHeader;
                const code = pick(header, 'responseCode', 'ResponseCode');
                if (code === '00') {
                    document.getElementById('modalAccountNumber').textContent = pick(data, 'accountNumber', 'AccountNumber') ?? '—';
                    document.getElementById('modalBankName').textContent = pick(data, 'bankName', 'BankName') ?? '—';
                    const firstName = pick(data, 'firstName', 'FirstName') ?? '';
                    const lastName = pick(data, 'lastName', 'LastName') ?? '';
                    document.getElementById('modalAccountName').textContent =
                        `${firstName} ${lastName}`.trim() || '—';
                    overlay.classList.add('active');
                    document.body.style.overflow = 'hidden';
                } else {
                    msgEl.textContent = header?.responseMessage || 'Unable to load account details.';
                    msgEl.className = 'fund-message error';
                }
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
    loadBalance().finally();
    initReceiptModal();
    initFundWalletModal();
});