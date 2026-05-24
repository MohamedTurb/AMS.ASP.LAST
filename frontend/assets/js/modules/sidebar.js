(function (global) {
  function init() {
    const toggle = document.querySelector("[data-sidebar-toggle]");
    const sidebar = document.querySelector(".ams-sidebar");
    const backdrop = document.querySelector(".ams-sidebar-backdrop");

    if (!toggle || !sidebar) return;

    function open() {
      sidebar.classList.add("is-open");
      backdrop?.classList.add("is-visible");
      document.body.style.overflow = "hidden";
    }

    function close() {
      sidebar.classList.remove("is-open");
      backdrop?.classList.remove("is-visible");
      document.body.style.overflow = "";
    }

    toggle.addEventListener("click", () => {
      sidebar.classList.contains("is-open") ? close() : open();
    });

    backdrop?.addEventListener("click", close);

    document.querySelectorAll(".ams-sidebar__link").forEach((link) => {
      link.addEventListener("click", () => {
        if (window.innerWidth < 992) close();
      });
    });
  }

  global.AmsSidebar = { init };
})(window);
