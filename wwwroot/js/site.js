// Password Visibility Toggle Script
document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (e) {
        const toggleBtn = e.target.closest('[data-toggle-password]');
        if (!toggleBtn) return;

        const targetId = toggleBtn.getAttribute('data-toggle-password');
        const input = document.getElementById(targetId);
        const icon = toggleBtn.querySelector('i');

        if (input && icon) {
            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        }
    });
});
