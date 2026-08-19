document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var page = document.querySelector('.cancel-page');
    if (!page) return;

    var vehicleType = (page.dataset.vehicleType || '').toLowerCase();

    async function cancelTrip(tripId) {
        if (!confirm('Cancel this trip? Refunds will be issued for affected bookings.')) return;

        try {
            var res = await fetch('/api/marshal/trips/cancel', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    tripId: tripId,
                    transportType: vehicleType,
                    reason: 'Cancelled by marshal'
                })
            });
            var data = await res.json();
            if (data.success) {
                alert(data.message);
                location.reload();
            } else {
                alert(data.message);
            }
        } catch (err) {
            alert('Network error');
        }
    }

    page.querySelectorAll('.btn-cancel').forEach(function (btn) {
        btn.addEventListener('click', function () {
            cancelTrip(btn.dataset.tripId);
        });
    });
});