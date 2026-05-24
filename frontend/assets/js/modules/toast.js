(function (global) {
  let container;

  function ensureContainer() {
    if (!container) {
      container = document.createElement("div");
      container.className = "ams-toast-container";
      container.setAttribute("aria-live", "polite");
      document.body.appendChild(container);
    }
    return container;
  }

  function show(message, type = "info", duration = 4500) {
    const el = document.createElement("div");
    el.className = "ams-toast";
    const icons = {
      success: "fa-check-circle text-success",
      error: "fa-exclamation-circle text-danger",
      info: "fa-info-circle text-primary",
    };
    el.innerHTML = `<i class="fas ${icons[type] || icons.info}"></i><span>${message}</span>`;
    ensureContainer().appendChild(el);
    setTimeout(() => {
      el.style.opacity = "0";
      el.style.transform = "translateY(8px)";
      setTimeout(() => el.remove(), 300);
    }, duration);
  }

  function initFromAlerts() {
    document.querySelectorAll("[data-ams-toast]").forEach((node) => {
      const type = node.dataset.amsToast || "info";
      show(node.textContent.trim(), type);
      node.remove();
    });
  }

  global.AmsToast = { show, initFromAlerts };
})(window);
