document.addEventListener('DOMContentLoaded', function () {
    const $ = id => document.getElementById(id);

    const cashOutBtn = $('cashOutBtn');
    const cashoutModal = $('cashoutModal');
    const confirmBtn = $('confirmCashoutBtn');
    const amountEl = $('cashoutAmount');
    const messageEl = $('cashoutMessage');
    const statusEl = $('cashoutStatus');
    const txnModal = $('txnModal');

    if (!cashOutBtn) {
        console.error('marshal-wallet: #cashOutBtn not found — cash out disabled.');
        return;
    }

    let bankAccount = null;

    function closeModals() {
        document.querySelectorAll('.modal-overlay').forEach(o => (o.style.display = 'none'));
    }

    document.querySelectorAll('[data-close-modal]').forEach(btn => btn.addEventListener('click', closeModals));
    document.querySelectorAll('.modal-overlay').forEach(o => o.addEventListener('click', e => {
        if (e.target === o) o.style.display = 'none';
    }));

    // ---- Transaction detail modal ----
    if (txnModal) {
        document.querySelectorAll('.txn-row').forEach(row => {
            row.addEventListener('click', () => {
                $('mType').textContent = row.dataset.type || '—';
                $('mAmount').textContent = row.dataset.amount || '—';
                $('mDescription').textContent = row.dataset.description || '—';
                $('mTransactionId').textContent = row.dataset.transactionId || '—';
                $('mSessionId').textContent = row.dataset.sessionId || '—';
                txnModal.style.display = 'flex';
            });
        });
    }

    // ---- Load linked payout account ----
    async function loadBankAccount() {
        try {
            const res = await fetch('/marshal/wallet/bankaccount', { headers: { Accept: 'application/json' } });
            const data = await res.json().catch(() => ({}));
            if (!res.ok) {
                return { error: data?.message || 'No bank account linked. Add one from your profile.' };
            }
            return data;
        } catch (err) {
            return { error: 'Network error. Could not load your bank account.' };
        }
    }

    cashOutBtn.addEventListener('click', async () => {
        // clear any previous state
        statusEl && (statusEl.textContent = '');
        messageEl.textContent = '';
        messageEl.className = 'enquiry-result';
        amountEl.value = '';

        cashOutBtn.disabled = true;
        const originalLabel = cashOutBtn.textContent;
        cashOutBtn.textContent = 'Loading…';

        try {
            if (!bankAccount) {
                const result = await loadBankAccount();
                if (result.error) {
                    // Show inline near the button — do NOT open a broken modal
                    if (statusEl) {
                        statusEl.textContent = result.error;
                        statusEl.className = 'enquiry-result error';
                    } else {
                        messageEl.textContent = result.error;
                        messageEl.className = 'enquiry-result error';
                        cashoutModal.style.display = 'flex';
                    }
                    return;
                }
                bankAccount = result;
            }

            $('cAccountName').textContent = bankAccount.accountName || '—';
            $('cAccountNumber').textContent = bankAccount.accountNumber || '—';
            $('cBankName').textContent = bankAccount.bankName || '—';
            cashoutModal.style.display = 'flex';
        } finally {
            cashOutBtn.disabled = false;
            cashOutBtn.textContent = originalLabel;
        }
    });

    confirmBtn.addEventListener('click', async () => {
        const amount = parseFloat(amountEl.value);
        if (!amount || amount <= 0) {
            messageEl.textContent = 'Enter a valid amount.';
            messageEl.className = 'enquiry-result error';
            return;
        }

        confirmBtn.disabled = true;
        confirmBtn.textContent = 'Processing…';
        messageEl.textContent = '';

        try {
            const res = await fetch('/marshal/wallet/cashout', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ amount })
            });
            const data = await res.json().catch(() => ({}));

            if (res.ok && data.success) {
                messageEl.textContent = 'Cash out successful.';
                messageEl.className = 'enquiry-result success';
                amountEl.value = '';
                setTimeout(() => location.reload(), 1500);
            } else {
                messageEl.textContent = data.message || 'Cash out failed.';
                messageEl.className = 'enquiry-result error';
            }
        } catch (err) {
            messageEl.textContent = 'Network error. Please try again.';
            messageEl.className = 'enquiry-result error';
        } finally {
            confirmBtn.disabled = false;
            confirmBtn.textContent = 'Cash Out';
        }
    });
});