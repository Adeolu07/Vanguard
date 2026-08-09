document.addEventListener('DOMContentLoaded', function () {
    const walletId = document.getElementById('walletId')?.value;
    const balance = document.getElementById('walletBalance');

    if (!walletId) {
        if (balance) balance.textContent = 'Wallet not linked';
        return;
    }

    // ---- Balance loader ----
    async function loadBalance() {
        try {
            const res = await fetch('/api/wallet/balance', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ customerId: walletId })
            });
            const data = await res.json();
            if (balance) {
                balance.textContent = data.responseHeader?.responseCode === '00'
                    ? `₦${data.balance.toLocaleString()}`
                    : '--';
            }
        } catch (err) {
            if (balance) balance.textContent = 'Error';
        }
    }

    // ---- Transaction receipt modal ----
    function omo() {
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

                openReceipt({ type, amount, description, transactionId, sessionId });

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

    // ---- Bank transfer modal ----
    const bankBtn = document.getElementById('fundBankTransferBtn');
    const bankOverlay = document.getElementById('bankTransferOverlay');
    const bankClose = document.getElementById('bankTransferClose');

    if (bankBtn && bankOverlay && bankClose) {
        bankBtn.addEventListener('click', async () => {
            bankOverlay.classList.add('active');
            document.body.style.overflow = 'hidden';

            document.getElementById('bankAccountName').textContent = 'Loading...';
            document.getElementById('bankAccountNumber').textContent = 'Loading...';
            document.getElementById('bankName').textContent = 'Loading...';
            document.getElementById('bankCode').textContent = 'Loading...';

            try {
                const res = await fetch('/api/wallet/nameenquiry', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ customerId: walletId })
                });
                const data = await res.json();
                if (data?.responseHeader?.responseCode === '00') {
                    document.getElementById('bankAccountName').textContent =
                        `${data.firstName ?? ''} ${data.lastName ?? ''}`.trim() || '—';
                    document.getElementById('bankAccountNumber').textContent = data.accountNumber || '—';
                    document.getElementById('bankName').textContent = data.bankName || '—';
                    document.getElementById('bankCode').textContent = data.bankCode || '—';
                } else {
                    throw new Error('Name Enquiry failed');
                }
            } catch (err) {
                console.error(err);
                document.getElementById('bankAccountName').textContent = 'Failed to load';
                document.getElementById('bankAccountNumber').textContent = '—';
                document.getElementById('bankName').textContent = '—';
                document.getElementById('bankCode').textContent = '—';
            }
        });

        bankClose.addEventListener('click', () => {
            bankOverlay.classList.remove('active');
            document.body.style.overflow = '';
        });
        bankOverlay.addEventListener('click', (e) => {
            if (e.target === bankOverlay) {
                bankOverlay.classList.remove('active');
                document.body.style.overflow = '';
            }
        });
    }

    const copyBtn = document.getElementById('bankCopyDetails');
    if (copyBtn) {
        copyBtn.addEventListener('click', () => {
            const details = `Account Number: ${document.getElementById('bankAccountNumber').textContent}\n`
            navigator.clipboard.writeText(details).then(() => {
                copyBtn.innerHTML = '✅ Copied';
                setTimeout(() => {
                    copyBtn.innerHTML = `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg> Copy Details`;
                }, 2000);
            }).catch(() => alert('Failed to copy'));
        });
    }

    loadBalance().finally();
    omo();
});