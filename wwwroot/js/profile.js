document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('profileForm');
    if (!form) return;

    const inputs = form.querySelectorAll('input[name]');
    const editBtn = document.getElementById('editBtn');
    const saveBtn = document.getElementById('saveBtn');
    const cancelBtn = document.getElementById('cancelBtn');

    if (!editBtn || !saveBtn || !cancelBtn) return;

    editBtn.addEventListener('click', function () {
        inputs.forEach(function (input) { input.disabled = false; });
        editBtn.style.display = 'none';
        saveBtn.style.display = 'inline-flex';
        cancelBtn.style.display = 'inline-flex';
    });

    cancelBtn.addEventListener('click', function () {
        inputs.forEach(function (input) {
            input.disabled = true;
            input.value = input.defaultValue;
        });
        saveBtn.style.display = 'none';
        cancelBtn.style.display = 'none';
        editBtn.style.display = 'inline-flex';
    });

    // ... existing code ...

// ---- Bank account verification (marshal payout account) ----
    const accountNumberInput = document.getElementById('accountNumber');
    const bankCodeSelect = document.getElementById('bankCode');
    const verifyBankBtn = document.getElementById('verifyBankBtn');
    const saveBankBtn = document.getElementById('saveBankBtn');
    const bankForm = document.getElementById('bankAccountForm');
    const verifyStatus = document.getElementById('bankVerifyStatus');
    const marshalNameEl = document.getElementById('marshalFullName');

    if (verifyBankBtn && bankForm) {
        // read a possibly camelCase/PascalCase JSON property
        function pick(obj, k1, k2) {
            if (!obj) return undefined;
            if (obj[k1] !== undefined) return obj[k1];
            if (obj[k2] !== undefined) return obj[k2];
            return undefined;
        }

        function normalizeName(value) {
            return (value || '').toUpperCase().replace(/[^A-Z0-9\s]/g, ' ').split(/\s+/).filter(Boolean);
        }

        // account name matches if every token of the user's name appears in it (order-insensitive)
        function namesMatch(userName, accountName) {
            const userTokens = normalizeName(userName);
            const accountTokens = normalizeName(accountName);
            if (userTokens.length === 0) return false;
            return userTokens.every(token => accountTokens.includes(token));
        }

        function setStatus(message, cls) {
            if (!verifyStatus) return;
            verifyStatus.textContent = message;
            verifyStatus.className = 'bank-verify-status ' + (cls || '');
        }

        function disableSave(message) {
            saveBankBtn.disabled = true;
            if (message) setStatus(message, 'error');
        }

        async function runVerification()
        {
            const accountNumber = accountNumberInput.value.trim();
            const bankCode = bankCodeSelect.value;
            const userName = marshalNameEl ? marshalNameEl.value.trim() : '';

            if (!/^\d{10}$/.test(accountNumber)) {
                disableSave('Enter a valid 10-digit account number.');
                return;
            }
            if (!bankCode) {
                disableSave('Select a bank.');
                return;
            }

            setStatus('Verifying account…', 'pending');
            verifyBankBtn.disabled = true;

            try {
                const res = await fetch('/api/cip/nameenquiry', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ accountNumber, bankCode })
                });
                const result = await res.json().catch(() => ({}));

                if (!result || !result.success || !result.data) {
                    disableSave((result && result.error) || 'Name enquiry failed.');
                    return;
                }

                const accountName = result.data.accountName;
                if (namesMatch(userName, accountName)) {
                    setStatus('Account name matches: ' + accountName, 'success');
                    saveBankBtn.disabled = false;
                } else {
                    disableSave('Account name "' + accountName + '" does not match your registered name.');
                }
            } catch (err) {
                disableSave('Network error. Please try again.');
            } finally {
                verifyBankBtn.disabled = false;
            }
        }

        verifyBankBtn.addEventListener('click', runVerification);
        accountNumberInput.addEventListener('input', () => disableSave('Account changed — verify again.'));
        bankCodeSelect.addEventListener('change', () => disableSave('Bank changed — verify again.'));

        // Block direct submit until verification passes
        bankForm.addEventListener('submit', function (e) {
            if (saveBankBtn.disabled) {
                e.preventDefault();
                setStatus('Verify your account before saving.', 'error');
            }
        });
    }
});

