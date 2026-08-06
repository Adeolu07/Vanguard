window.addEventListener('DOMContentLoaded', function() {
    const statusMsg = document.getElementById('statusMsg');
    if (statusMsg) {
        setTimeout(async () => {
            statusMsg.textContent = 'Confirming transaction...';
            try {
                const res = await fetch('/Wallet/VerifyPayment');
                const data = await res.json();
                window.location.href = `/Wallet/PaymentResult?success=${data.success}&amount=${data.amount}`;
            } catch {
                window.location.href = '/Wallet/PaymentResult?success=false&amount=0';
            }
        }, 2000);
    }
});