// Birkan Uysal — site davranışları
(() => {
  "use strict";

  // Mobil menü
  const birkanLinks = document.getElementById("birkan-navbar-links");
  const birkanToggle = document.getElementById("birkan-navbar-toggle");

  function birkanSetMenu(open) {
    birkanLinks.classList.toggle("birkan-is-open", open);
    birkanToggle.setAttribute("aria-expanded", String(open));
  }

  birkanToggle.addEventListener("click", () => birkanSetMenu(!birkanLinks.classList.contains("birkan-is-open")));
  birkanLinks.addEventListener("click", (e) => {
    if (e.target.closest("a")) birkanSetMenu(false);
  });

  // Menüde o an okunan bölümü işaretle
  const birkanSpyLinks = new Map();
  document.querySelectorAll("[data-birkan-spy]").forEach((a) => birkanSpyLinks.set(a.dataset.birkanSpy, a));

  const birkanSpy = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      birkanSpyLinks.forEach((a) => a.classList.remove("birkan-is-active"));
      birkanSpyLinks.get(entry.target.id)?.classList.add("birkan-is-active");
    }
  }, { rootMargin: "-40% 0px -55% 0px" });
  birkanSpyLinks.forEach((_, id) => {
    const section = document.getElementById(id);
    if (section) birkanSpy.observe(section);
  });

  // Proje detay pencereleri
  document.querySelectorAll("[data-birkan-open]").forEach((button) => {
    button.addEventListener("click", () => {
      const dialog = document.getElementById(button.dataset.birkanOpen);
      if (!dialog) return;
      dialog.showModal();
      document.body.classList.add("birkan-has-dialog");
    });
  });

  document.querySelectorAll(".birkan-dialog").forEach((dialog) => {
    dialog.addEventListener("click", (e) => {
      // Pencerenin dışına tıklanınca kapanıyor.
      const r = dialog.getBoundingClientRect();
      const outside = e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom;
      if (outside || e.target.closest("[data-birkan-close]")) dialog.close();
    });
    dialog.addEventListener("close", () => document.body.classList.remove("birkan-has-dialog"));
  });
})();
