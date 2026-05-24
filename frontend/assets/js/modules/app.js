(function (global) {
  function initProgressBars() {
    document.querySelectorAll("[data-progress]").forEach((bar) => {
      const value = parseInt(bar.getAttribute("data-progress") || "0", 10);
      bar.style.width = `${Math.min(100, Math.max(0, value))}%`;
    });
  }

  function initActiveNav() {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll(".ams-sidebar__link").forEach((link) => {
      const href = (link.getAttribute("href") || "").toLowerCase();
      if (href && href !== "/" && path.startsWith(href)) {
        link.classList.add("is-active");
      } else if (href === "/" && (path === "/" || path.endsWith("/home/index"))) {
        link.classList.add("is-active");
      }
    });
  }

  function initFileUploadZones() {
    document.querySelectorAll(".ams-upload-zone").forEach((zone) => {
      const input = zone.querySelector('input[type="file"]');
      if (!input) return;

      ["dragenter", "dragover"].forEach((ev) => {
        zone.addEventListener(ev, (e) => {
          e.preventDefault();
          zone.classList.add("is-dragover");
        });
      });
      ["dragleave", "drop"].forEach((ev) => {
        zone.addEventListener(ev, (e) => {
          e.preventDefault();
          zone.classList.remove("is-dragover");
        });
      });
      zone.addEventListener("drop", (e) => {
        if (e.dataTransfer?.files?.length) {
          input.files = e.dataTransfer.files;
          zone.querySelector(".ams-upload-zone__name")?.replaceChildren(
            document.createTextNode(e.dataTransfer.files[0].name)
          );
        }
      });
      input.addEventListener("change", () => {
        if (input.files?.[0]) {
          const label = zone.querySelector(".ams-upload-zone__name");
          if (label) label.textContent = input.files[0].name;
        }
      });
    });
  }

  function init() {
    initProgressBars();
    initActiveNav();
    initFileUploadZones();
    if (global.AmsTheme) global.AmsTheme.init();
    if (global.AmsSidebar) global.AmsSidebar.init();
    if (global.AmsToast) global.AmsToast.initFromAlerts();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  global.AmsApp = { init };
})(window);
