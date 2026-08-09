document.addEventListener('DOMContentLoaded', function () {
    const walletId = document.getElementById('walletId')?.value;
    const balance = document.getElementById('walletBalance');

    if (!walletId) {
        if (balance) 
            balance.textContent = 'Wallet not linked';
        return;
    }

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
                balance.textContent = data.responseHeader?.responseCode === '00'
                    ? '₦' + data.balance.toLocaleString()
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
            if (e.target === overlay) closeModal();
        });

        printBtn.addEventListener('click', function () {
            window.print();
        });
    }

    // ---- Fund wallet ----
    function initFundButton() {
        const fundBtn = document.getElementById('fundButton');
        const amountEl = document.getElementById('fundAmount');
        const descEl = document.getElementById('fundDescription');
        const msgEl = document.getElementById('fundMessage');
        if (!fundBtn || !amountEl || !msgEl) 
            return;

        fundBtn.addEventListener('click', async () => {
            const amount = parseFloat(amountEl.value);
            if (!amount || amount <= 0) {
                msgEl.textContent = 'Please enter a valid amount.';
                msgEl.className = 'fund-message error';
                return;
            }

            fundBtn.disabled = true;
            fundBtn.textContent = 'Processing…';
            msgEl.textContent = '';

            try {
                const traceId = crypto.randomUUID();
                const res = await fetch('/api/wallet/credit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        CustomerId: walletId,
                        Amount: amount,
                        Description: descEl.value.trim() || 'Fund wallet',
                        TraceId:traceId
                    })
                });
                const data = await res.json();

                if (res.ok && data.responseHeader && data.responseHeader.responseCode === '00') {
                    msgEl.textContent = 'Funds added successfully!';
                    msgEl.className = 'fund-message success';
                    // Refresh balance display
                    const newBalance = data.balance ?? (await fetchBalance(walletId));
                    if (newBalance !== undefined) {
                        document.getElementById('walletBalance').textContent =
                            '₦' + parseFloat(newBalance).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                    }
                    amountEl.value = '';
                    descEl.value = '';
                } else {
                    const errMsg = data.responseHeader?.responseMessage || data.message || 'Credit failed.';
                    msgEl.textContent = errMsg;
                    msgEl.className = 'fund-message error';
                }
            } catch (err) {
                msgEl.textContent = 'Network error. Please try again.';
                msgEl.className = 'fund-message error';
            } finally {
                fundBtn.disabled = false;
                fundBtn.textContent = 'Add Funds';
            }
        });

        async function fetchBalance(customerId) {
            const res = await fetch('/api/wallet/balance', {
                method: 'POST',
                headers: { '2Content-Type': 'application/json' },
                body: JSON.stringify({ customerId })
            });
            const data = await res.json();
            return data.balance;
        }
    }

    // ---- Boot ----
    loadBalance().finally();
    initReceiptModal();
    initFundButton();
});