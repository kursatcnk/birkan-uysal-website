// Birkan Uysal — site davranışları
(() => {
  "use strict";

  // Üst menü girişte şeffaf; sayfa kaydıkça grafit zemin alıyor.
  const birkanNavbar = document.getElementById("birkan-navbar");
  const birkanOnScroll = () => birkanNavbar.classList.toggle("birkan-is-scrolled", window.scrollY > 40);
  window.addEventListener("scroll", birkanOnScroll, { passive: true });
  birkanOnScroll();

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

  // Bölümler ekrana girerken yumuşakça beliriyor.
  const birkanRevealer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      entry.target.classList.add("birkan-is-visible");
      birkanRevealer.unobserve(entry.target);
    }
  }, { rootMargin: "0px 0px -10% 0px" });
  document.querySelectorAll("[data-birkan-reveal]").forEach((el) => birkanRevealer.observe(el));

  // Proje detayları sağdan açılan panelde
  document.querySelectorAll("[data-birkan-open]").forEach((button) => {
    button.addEventListener("click", () => {
      const drawer = document.getElementById(button.dataset.birkanOpen);
      if (!drawer) return;
      drawer.showModal();
      document.body.classList.add("birkan-has-dialog");
    });
  });

  document.querySelectorAll(".birkan-drawer").forEach((drawer) => {
    drawer.addEventListener("click", (e) => {
      // Panelin dışına (karartılmış alana) tıklanınca kapanıyor.
      const r = drawer.getBoundingClientRect();
      const outside = e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom;
      if (outside || e.target.closest("[data-birkan-close]")) drawer.close();
    });
    drawer.addEventListener("close", () => document.body.classList.remove("birkan-has-dialog"));
  });
})();
