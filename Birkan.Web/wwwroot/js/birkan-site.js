// Birkan Uysal — site davranışları
(() => {
  "use strict";

  const birkanBody = document.body;
  const birkanReduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  // Ön yükleyici kısa; sayfa hazır olunca kalkıyor, isim satırları yukarı kayarak geliyor.
  function birkanFinishLoading() {
    birkanBody.classList.remove("birkan-is-loading");
    requestAnimationFrame(() => birkanBody.classList.add("birkan-is-ready"));
  }
  if (document.readyState === "complete") {
    setTimeout(birkanFinishLoading, birkanReduced ? 0 : 500);
  } else {
    window.addEventListener("load", () => setTimeout(birkanFinishLoading, birkanReduced ? 0 : 500));
  }
  // Görseller yavaş gelirse kullanıcıyı ön yükleyicide bekletmemek için üst sınır.
  setTimeout(birkanFinishLoading, 2200);

  // Üst menü: aşağı kaydırınca saklanıyor, yukarı kaydırınca geri geliyor.
  const birkanNavbar = document.getElementById("birkan-navbar");
  const birkanLinks = document.getElementById("birkan-navbar-links");
  const birkanToggle = document.getElementById("birkan-navbar-toggle");
  let birkanLastY = window.scrollY;

  function birkanOnNavScroll() {
    const y = window.scrollY;
    birkanNavbar.classList.toggle("birkan-is-scrolled", y > 24);
    const menuOpen = birkanLinks.classList.contains("birkan-is-open");
    birkanNavbar.classList.toggle("birkan-is-hidden", !menuOpen && y > 400 && y > birkanLastY);
    birkanLastY = y;
  }

  function birkanSetMenu(open) {
    birkanLinks.classList.toggle("birkan-is-open", open);
    birkanNavbar.classList.toggle("birkan-is-menu-open", open);
    birkanToggle.setAttribute("aria-expanded", String(open));
  }

  birkanToggle.addEventListener("click", () => birkanSetMenu(!birkanLinks.classList.contains("birkan-is-open")));
  birkanLinks.addEventListener("click", (e) => {
    if (e.target.closest("a")) birkanSetMenu(false);
  });

  // Giriş fotoğrafı: sahne boyunca kaydırdıkça çerçeve kenarlara açılıyor.
  const birkanStage = document.getElementById("birkan-hero-stage");
  const birkanFrame = birkanStage?.querySelector(".birkan-hero-frame");

  function birkanOnHeroScroll() {
    if (!birkanFrame || birkanReduced) return;
    const rect = birkanStage.getBoundingClientRect();
    const travel = rect.height - window.innerHeight;
    const progress = Math.min(1, Math.max(0, (window.innerHeight * 0.6 - rect.top) / (travel + window.innerHeight * 0.6)));
    birkanFrame.style.setProperty("--birkan-expand", progress.toFixed(4));
  }

  let birkanTicking = false;
  window.addEventListener("scroll", () => {
    if (birkanTicking) return;
    birkanTicking = true;
    requestAnimationFrame(() => {
      birkanOnNavScroll();
      birkanOnHeroScroll();
      birkanTicking = false;
    });
  }, { passive: true });
  birkanOnNavScroll();
  birkanOnHeroScroll();

  // Kaydırınca beliren öğeler
  const birkanRevealer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      entry.target.classList.add("birkan-is-visible");
      birkanRevealer.unobserve(entry.target);
    }
  }, { rootMargin: "0px 0px -8% 0px" });
  document.querySelectorAll("[data-birkan-reveal]").forEach((el) => birkanRevealer.observe(el));

  // Menüde o an ekranda olan bölümü işaretle
  const birkanSpyLinks = new Map();
  document.querySelectorAll("[data-birkan-spy]").forEach((a) => birkanSpyLinks.set(a.dataset.birkanSpy, a));

  const birkanSpy = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      birkanSpyLinks.forEach((a) => a.classList.remove("birkan-is-active"));
      birkanSpyLinks.get(entry.target.id)?.classList.add("birkan-is-active");
    }
  }, { rootMargin: "-45% 0px -50% 0px" });
  birkanSpyLinks.forEach((_, id) => {
    const section = document.getElementById(id);
    if (section) birkanSpy.observe(section);
  });

  // Proje detay pencereleri
  function birkanCloseDialog(dialog) {
    dialog.close();
    birkanBody.classList.remove("birkan-has-dialog");
  }

  document.querySelectorAll("[data-birkan-open]").forEach((button) => {
    button.addEventListener("click", () => {
      const dialog = document.getElementById(button.dataset.birkanOpen);
      if (!dialog) return;
      dialog.showModal();
      birkanBody.classList.add("birkan-has-dialog");
    });
  });

  document.querySelectorAll(".birkan-dialog").forEach((dialog) => {
    dialog.addEventListener("click", (e) => {
      // Pencerenin dışına (arka plana) tıklanınca kapanıyor.
      if (e.target === dialog || e.target.closest("[data-birkan-close]")) birkanCloseDialog(dialog);
    });
    dialog.addEventListener("close", () => birkanBody.classList.remove("birkan-has-dialog"));
  });
})();
