// Birkan Uysal — site davranışları ve hareketleri
(() => {
  "use strict";

  const birkanRoot = document.documentElement;
  const birkanCanAnimate = birkanRoot.classList.contains("birkan-anim") && window.gsap && window.ScrollTrigger;
  const birkanFinePointer = window.matchMedia("(hover: hover) and (pointer: fine)").matches;

  // ---------- Hareketten bağımsız temel davranışlar ----------

  // Üst menü: girişte şeffaf, kaydırınca zemin alıyor; aşağı inerken saklanıp yukarı çıkınca geri geliyor.
  const birkanNavbar = document.getElementById("birkan-navbar");
  const birkanProgress = document.querySelector(".birkan-progress");
  let birkanLastY = window.scrollY;

  function birkanOnScroll() {
    const y = window.scrollY;
    const menuOpen = birkanLinks.classList.contains("birkan-is-open");
    birkanNavbar.classList.toggle("birkan-is-scrolled", y > 40);
    // Yumuşak kaydırmada son karelerde fark çok küçük kalıyor; yön yalnızca belirgin harekette değiştiriliyor.
    if (!menuOpen && y > 500 && y > birkanLastY + 2) birkanNavbar.classList.add("birkan-is-hidden");
    else if (y < birkanLastY - 2 || y <= 500 || menuOpen) birkanNavbar.classList.remove("birkan-is-hidden");
    birkanLastY = y;

    const max = document.documentElement.scrollHeight - window.innerHeight;
    birkanProgress.style.transform = `scaleX(${max > 0 ? y / max : 0})`;
  }

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

  window.addEventListener("scroll", birkanOnScroll, { passive: true });
  birkanOnScroll();

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

  // İletişim bağlantılarını harflere böl: üzerine gelince harfler sırayla yükseliyor.
  document.querySelectorAll(".birkan-contact-list a").forEach((a) => {
    const text = a.textContent;
    a.setAttribute("aria-label", text);
    a.textContent = "";
    [...text].forEach((ch, i) => {
      const span = document.createElement("span");
      span.className = "birkan-char";
      span.style.setProperty("--i", i);
      span.setAttribute("aria-hidden", "true");
      span.textContent = ch;
      a.appendChild(span);
    });
  });

  // Proje kartlarındaki çizimler üzerine gelince baştan çizilebilsin diye uzunlukları 1'e normalleştiriliyor.
  document.querySelectorAll(".birkan-drawing circle, .birkan-drawing path, .birkan-drawing line, .birkan-drawing rect").forEach((el) => {
    if (!el.matches(".birkan-drawing-axis, .birkan-drawing-pitch, .birkan-drawing-fill")) el.setAttribute("pathLength", "1");
  });

  // Proje detayları sağdan açılan panelde
  let birkanLenis = null;

  document.querySelectorAll("[data-birkan-open]").forEach((button) => {
    button.addEventListener("click", () => {
      const drawer = document.getElementById(button.dataset.birkanOpen);
      if (!drawer) return;
      drawer.showModal();
      document.body.classList.add("birkan-has-dialog");
      birkanLenis?.stop();
    });
  });

  document.querySelectorAll(".birkan-drawer").forEach((drawer) => {
    drawer.addEventListener("click", (e) => {
      // Panelin dışına (karartılmış alana) tıklanınca kapanıyor.
      const r = drawer.getBoundingClientRect();
      const outside = e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom;
      if (outside || e.target.closest("[data-birkan-close]")) drawer.close();
    });
    drawer.addEventListener("close", () => {
      document.body.classList.remove("birkan-has-dialog");
      birkanLenis?.start();
    });
  });

  if (!birkanCanAnimate) {
    birkanRoot.classList.add("birkan-intro-done");
    return;
  }

  // ---------- Hareketler ----------

  const gsap = window.gsap;
  gsap.registerPlugin(window.ScrollTrigger);
  const ScrollTrigger = window.ScrollTrigger;

  // Yumuşak kaydırma; menü bağlantıları da aynı yumuşaklıkla kayıyor.
  if (window.Lenis) {
    birkanLenis = new window.Lenis({ lerp: 0.085, smoothWheel: true });
    birkanLenis.on("scroll", ScrollTrigger.update);
    gsap.ticker.add((time) => birkanLenis.raf(time * 1000));
    gsap.ticker.lagSmoothing(0);
    birkanLenis.stop();

    document.querySelectorAll('a[href^="#"]').forEach((a) => {
      a.addEventListener("click", (e) => {
        const target = document.querySelector(a.getAttribute("href"));
        if (!target) return;
        e.preventDefault();
        birkanLenis.scrollTo(target, { offset: target.id === "birkan-home" ? 0 : -72, duration: 1.4 });
      });
    });
  }

  // Metni kelimelere (ya da harflere) böler; <em> gibi iç öğeler korunuyor.
  function birkanSplit(el, byChar) {
    const parts = [];
    const walk = (node) => {
      [...node.childNodes].forEach((child) => {
        if (child.nodeType === Node.TEXT_NODE) {
          const frag = document.createDocumentFragment();
          child.textContent.split(/(\s+)/).forEach((token) => {
            if (!token) return;
            if (/^\s+$/.test(token)) {
              frag.appendChild(document.createTextNode(" "));
              return;
            }
            const pieces = byChar ? [...token] : [token];
            const wordWrap = byChar ? document.createElement("span") : null;
            if (wordWrap) wordWrap.style.whiteSpace = "nowrap";
            pieces.forEach((p) => {
              const mask = document.createElement("span");
              mask.className = "birkan-mask";
              const inner = document.createElement("span");
              inner.textContent = p;
              mask.appendChild(inner);
              (wordWrap ?? frag).appendChild(mask);
              parts.push(inner);
            });
            if (wordWrap) frag.appendChild(wordWrap);
          });
          child.replaceWith(frag);
        } else if (child.nodeType === Node.ELEMENT_NODE) {
          walk(child);
        }
      });
    };
    walk(el);
    return parts;
  }

  // Açılış: isim harf harf beliriyor, perde yukarı çekiliyor, ardından giriş bölümü kuruluyor.
  const introChars = birkanSplit(document.querySelector(".birkan-intro-name"), true);
  const heroChars = birkanSplit(document.querySelector(".birkan-hero-name"), true);
  const heroPortrait = document.querySelector(".birkan-hero-portrait");
  const heroImg = heroPortrait?.querySelector("img");

  gsap.set(heroChars, { yPercent: 115 });
  gsap.set([".birkan-hero-meta", ".birkan-hero-intro", ".birkan-hero-actions > *"], { y: 24, opacity: 0 });
  gsap.set(heroPortrait, { clipPath: "inset(100% 0% 0% 0%)" });
  gsap.set(heroImg, { scale: 1.35 });

  const intro = gsap.timeline({
    defaults: { ease: "power4.out" },
    onComplete: () => {
      birkanRoot.classList.add("birkan-intro-done");
      birkanLenis?.start();
      ScrollTrigger.refresh();
    }
  });

  intro
    .from(introChars, { yPercent: 115, duration: 0.85, stagger: 0.028 })
    .from(".birkan-intro-role", { y: 12, opacity: 0, duration: 0.6 }, "-=0.45")
    .to(".birkan-intro", { yPercent: -100, duration: 1.05, ease: "power4.inOut" }, "+=0.25")
    .to(heroPortrait, { clipPath: "inset(0% 0% 0% 0%)", duration: 1.4, ease: "power4.inOut" }, "-=0.7")
    .to(heroImg, { scale: 1.08, duration: 1.8, ease: "power3.out" }, "<")
    .to(heroChars, { yPercent: 0, duration: 1.1, stagger: 0.032 }, "-=1.45")
    .to(".birkan-hero-meta", { y: 0, opacity: 1, duration: 0.9 }, "-=0.9")
    .to(".birkan-hero-intro", { y: 0, opacity: 1, duration: 0.9 }, "-=0.75")
    .to(".birkan-hero-actions > *", { y: 0, opacity: 1, duration: 0.8, stagger: 0.08 }, "-=0.7");

  // Girişten çıkarken portre yavaş, metin hızlı kayıyor (derinlik hissi).
  gsap.to(heroImg, {
    yPercent: 10,
    ease: "none",
    scrollTrigger: { trigger: ".birkan-hero", start: "top top", end: "bottom top", scrub: true }
  });
  gsap.to(".birkan-hero-copy", {
    y: -80,
    opacity: 0.25,
    ease: "none",
    scrollTrigger: { trigger: ".birkan-hero", start: "top top", end: "bottom top", scrub: true }
  });

  // Başlıklar kelime kelime maskeden yükseliyor.
  document.querySelectorAll("[data-birkan-split]").forEach((el) => {
    const words = birkanSplit(el, false);
    gsap.from(words, {
      yPercent: 110,
      duration: 1.1,
      stagger: 0.07,
      ease: "power4.out",
      scrollTrigger: { trigger: el, start: "top 88%" }
    });
  });

  // Tek tek beliren bloklar
  document.querySelectorAll("[data-birkan-reveal]").forEach((el) => {
    if (el.closest("[data-birkan-stagger]")) return;
    const targets = el.querySelector("[data-birkan-split]")
      ? [...el.children].filter((c) => !c.matches("[data-birkan-split]"))
      : [el];
    if (!targets.length) return;
    gsap.from(targets, {
      y: 40,
      opacity: 0,
      duration: 1.1,
      ease: "power3.out",
      scrollTrigger: { trigger: el, start: "top 88%" }
    });
  });

  // Listeler: üst çizgi soldan sağa çiziliyor, öğeler sırayla geliyor.
  const birkanRuleFrom = (el, trigger) =>
    gsap.fromTo(el, { "--birkan-rule": "0%" }, {
      "--birkan-rule": "100%",
      duration: 1.5,
      ease: "power3.inOut",
      scrollTrigger: { trigger: trigger ?? el, start: "top 88%" }
    });

  document.querySelectorAll("[data-birkan-stagger]").forEach((list) => {
    birkanRuleFrom(list);
    gsap.from(list.children, {
      y: 46,
      opacity: 0,
      duration: 1.1,
      stagger: 0.09,
      ease: "power3.out",
      scrollTrigger: { trigger: list, start: "top 86%" }
    });
  });

  const expertise = document.querySelector(".birkan-expertise ul");
  if (expertise) {
    birkanRuleFrom(expertise);
    gsap.from(expertise.children, {
      x: -16,
      opacity: 0,
      duration: 0.9,
      stagger: 0.05,
      ease: "power3.out",
      scrollTrigger: { trigger: expertise, start: "top 88%" }
    });
  }
  const aboutTop = document.querySelector(".birkan-about-top");
  if (aboutTop) birkanRuleFrom(aboutTop);

  // İletişim bölümüne girerken zemin aşağıdan yukarı açılıyor.
  gsap.from(".birkan-contact", {
    clipPath: "inset(12% 4% 0% 4%)",
    ease: "none",
    scrollTrigger: { trigger: ".birkan-contact", start: "top bottom", end: "top 35%", scrub: true }
  });

  // Fare ile ince dokunuşlar: ana düğme mıknatıs gibi, proje kartları hafif eğiliyor.
  if (birkanFinePointer) {
    document.querySelectorAll(".birkan-btn").forEach((btn) => {
      const xTo = gsap.quickTo(btn, "x", { duration: 0.5, ease: "power3.out" });
      const yTo = gsap.quickTo(btn, "y", { duration: 0.5, ease: "power3.out" });
      btn.addEventListener("mousemove", (e) => {
        const r = btn.getBoundingClientRect();
        xTo((e.clientX - r.left - r.width / 2) * 0.35);
        yTo((e.clientY - r.top - r.height / 2) * 0.45);
      });
      btn.addEventListener("mouseleave", () => {
        gsap.to(btn, { x: 0, y: 0, duration: 0.9, ease: "elastic.out(1, 0.45)" });
      });
    });

    document.querySelectorAll(".birkan-project").forEach((card) => {
      const rx = gsap.quickTo(card, "rotationX", { duration: 0.6, ease: "power3.out" });
      const ry = gsap.quickTo(card, "rotationY", { duration: 0.6, ease: "power3.out" });
      gsap.set(card, { transformPerspective: 1100 });
      card.addEventListener("mousemove", (e) => {
        const r = card.getBoundingClientRect();
        ry(((e.clientX - r.left) / r.width - 0.5) * 7);
        rx(-((e.clientY - r.top) / r.height - 0.5) * 7);
      });
      card.addEventListener("mouseleave", () => { rx(0); ry(0); });
    });
  }

  // Yazı tipleri yüklendiğinde ölçüler değişebiliyor; tetikleyiciler yeniden hesaplanıyor.
  document.fonts?.ready.then(() => ScrollTrigger.refresh());
})();
