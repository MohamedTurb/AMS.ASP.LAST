(function () {
    function clampProgress(value) {
        const parsed = Number(value);
        if (Number.isNaN(parsed)) {
            return 0;
        }

        return Math.max(0, Math.min(100, parsed));
    }

    function syncProgressBars() {
        document.querySelectorAll('.progress-bar[data-progress]').forEach((bar) => {
            const progress = clampProgress(bar.dataset.progress);
            bar.style.width = `${progress}%`;
            bar.setAttribute('aria-valuenow', String(progress));
        });
    }

    function collapseNavbarOnNavigate() {
        const navbarCollapse = document.querySelector('.navbar-collapse');
        const navbarToggler = document.querySelector('.navbar-toggler');

        if (!navbarCollapse || !navbarToggler || !window.bootstrap || !bootstrap.Collapse) {
            return;
        }

        const collapseInstance = bootstrap.Collapse.getOrCreateInstance(navbarCollapse, { toggle: false });
        document.querySelectorAll('.navbar-nav .nav-link').forEach((link) => {
            link.addEventListener('click', () => {
                if (window.getComputedStyle(navbarToggler).display !== 'none' && navbarCollapse.classList.contains('show')) {
                    collapseInstance.hide();
                }
            });
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        syncProgressBars();
        collapseNavbarOnNavigate();
    });
})();