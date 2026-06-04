// Submit loading state — primary buttons only (skips danger/delete buttons)
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            var btn = form.querySelector('button[type="submit"].btn--primary');
            if (btn && !btn.disabled) {
                btn.disabled = true;
                btn.insertAdjacentHTML('beforeend', '<i class="btn-spinner fa-solid fa-circle-notch fa-spin"></i>');
            }
        });
    });
});
