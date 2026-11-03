// Birkan Uysal — yönetim paneli davranışları
(() => {
  "use strict";

  const birkanToken = () =>
    document.querySelector('#birkan-upload-token input[name="__RequestVerificationToken"]')?.value ?? "";

  // ---- Tekrarlanan gruplar: ekle, sil, taşı ----

  function birkanRefreshIndexes(scope) {
    scope.querySelectorAll("[data-birkan-list]").forEach((list) => {
      [...list.children].filter((el) => el.matches("[data-birkan-item]")).forEach((item, i) => {
        const index = item.querySelector(":scope > .birkan-item-head [data-birkan-index]");
        if (index) index.textContent = String(i + 1).padStart(2, "0");
      });
    });
  }

  function birkanSyncTitle(item) {
    const source = item.querySelector(":scope [data-birkan-title-source] input");
    const title = item.querySelector(":scope > .birkan-item-head [data-birkan-title]");
    if (source && title && source.value.trim()) title.textContent = source.value.trim();
  }

  document.addEventListener("click", (e) => {
    const add = e.target.closest("[data-birkan-add]");
    if (add) {
      const repeater = add.closest("[data-birkan-repeater]");
      const template = repeater.querySelector(":scope > template[data-birkan-template]");
      const list = repeater.querySelector(":scope > [data-birkan-list]");
      const node = template.content.firstElementChild.cloneNode(true);
      list.appendChild(node);
      repeater.parentElement.querySelector(":scope > [data-birkan-empty]")?.remove();
      birkanBindImages(node);
      birkanBindAutosize(node);
      birkanRefreshIndexes(repeater.closest("form") ?? document);
      birkanMarkDirty();
      node.querySelector("input:not([type=hidden]):not([type=file]), textarea")?.focus();
      return;
    }

    const remove = e.target.closest("[data-birkan-remove]");
    if (remove) {
      const item = remove.closest("[data-birkan-item]");
      const label = item.querySelector("[data-birkan-title]")?.textContent?.trim();
      const isLine = item.classList.contains("birkan-line");
      const hasText = [...item.querySelectorAll("input, textarea")].some((f) => f.type !== "hidden" && f.type !== "file" && f.value.trim());
      if (!isLine && hasText && !confirm(`"${label}" silinsin mi? Kaydetmeden önce sayfadan çıkarsanız silme işlemi uygulanmaz.`)) return;
      const form = item.closest("form");
      item.remove();
      birkanRefreshIndexes(form ?? document);
      birkanMarkDirty();
      return;
    }

    const move = e.target.closest("[data-birkan-move]");
    if (move) {
      const item = move.closest("[data-birkan-item]");
      const dir = Number(move.dataset.birkanMove);
      const sibling = dir < 0 ? item.previousElementSibling : item.nextElementSibling;
      if (!sibling || !sibling.matches("[data-birkan-item]")) return;
      if (dir < 0) sibling.before(item);
      else sibling.after(item);
      birkanRefreshIndexes(item.closest("form") ?? document);
      birkanMarkDirty();
      item.animate([{ backgroundColor: "rgba(200,16,46,0.08)" }, { backgroundColor: "transparent" }], { duration: 700 });
    }
  });

  // Gönderimden hemen önce her alana tam adını ver: Items[1].Highlights[0] gibi.
  function birkanAssignNames(form) {
    form.querySelectorAll("[data-birkan-name]").forEach((field) => {
      if (field.closest("template")) return;
      const segments = [];
      let node = field.closest("[data-birkan-item]");
      while (node) {
        const list = node.parentElement;
        const repeater = list.closest("[data-birkan-repeater]");
        const index = [...list.children].filter((el) => el.matches("[data-birkan-item]")).indexOf(node);
        segments.unshift(`${repeater.dataset.birkanRepeater}[${index}]`);
        node = repeater.parentElement.closest("[data-birkan-item]");
      }
      const rel = field.dataset.birkanName;
      field.name = segments.join(".") + (rel ? `.${rel}` : "");
    });
  }

  // ---- Kaydedilmemiş değişiklik uyarısı ----

  let birkanDirty = false;
  function birkanMarkDirty() {
    if (birkanDirty) return;
    birkanDirty = true;
    document.body.classList.add("birkan-is-dirty");
    document.querySelectorAll("[data-birkan-dirty-label]").forEach((el) => (el.textContent = "Kaydedilmemiş değişiklikler var"));
  }

  document.querySelectorAll("[data-birkan-form]").forEach((form) => {
    form.addEventListener("input", (e) => {
      birkanMarkDirty();
      const item = e.target.closest("[data-birkan-item]");
      if (item && e.target.closest("[data-birkan-title-source]")) birkanSyncTitle(item);
    });
    form.addEventListener("submit", () => {
      birkanAssignNames(form);
      birkanDirty = false;
    });
  });

  window.addEventListener("beforeunload", (e) => {
    if (!birkanDirty) return;
    e.preventDefault();
    e.returnValue = "";
  });

  // ---- Görsel yükleme ----

  function birkanBindImages(scope) {
    scope.querySelectorAll("[data-birkan-image]").forEach((field) => {
      if (field.dataset.birkanBound) return;
      field.dataset.birkanBound = "1";

      const value = field.querySelector("[data-birkan-image-value]");
      const preview = field.querySelector(".birkan-image-preview");
      const status = field.querySelector("[data-birkan-image-status]");
      const file = field.querySelector("[data-birkan-image-file]");

      const isDocument = field.dataset.kind === "document";

      const show = (path) => {
        preview.innerHTML = "";
        if (path && isDocument) {
          const link = document.createElement("a");
          link.href = path;
          link.target = "_blank";
          link.rel = "noopener";
          link.className = "birkan-doc-link";
          link.textContent = "PDF";
          preview.appendChild(link);
        } else if (path) {
          const img = document.createElement("img");
          img.src = path;
          img.alt = "";
          preview.appendChild(img);
        } else {
          const empty = document.createElement("span");
          empty.className = "birkan-image-empty";
          empty.textContent = isDocument ? "Belge yok" : "Görsel yok";
          preview.appendChild(empty);
        }
      };

      file.addEventListener("change", async () => {
        if (!file.files.length) return;
        const data = new FormData();
        data.append("file", file.files[0]);
        data.append("kind", field.dataset.kind);
        status.textContent = "Yükleniyor…";
        field.classList.add("birkan-is-busy");
        try {
          const res = await fetch("/birkan-panel/yukle", {
            method: "POST",
            body: data,
            headers: { RequestVerificationToken: birkanToken() }
          });
          const json = await res.json().catch(() => ({}));
          if (!res.ok) throw new Error(json.error || "Yükleme başarısız oldu.");
          value.value = json.path;
          show(json.path);
          status.textContent = "Yüklendi, kaydetmeyi unutmayın.";
          birkanMarkDirty();
        } catch (err) {
          status.textContent = err.message;
        } finally {
          field.classList.remove("birkan-is-busy");
          file.value = "";
        }
      });

      field.querySelector("[data-birkan-image-clear]").addEventListener("click", () => {
        value.value = "";
        show("");
        status.textContent = "";
        birkanMarkDirty();
      });
    });
  }

  // ---- Uzun metin kutuları içerik kadar uzasın ----

  function birkanBindAutosize(scope) {
    scope.querySelectorAll("[data-birkan-autosize]").forEach((area) => {
      const fit = () => {
        area.style.height = "auto";
        area.style.height = `${area.scrollHeight + 2}px`;
      };
      area.addEventListener("input", fit);
      requestAnimationFrame(fit);
    });
  }

  // ---- Onay isteyen formlar, bildirim, mobil menü ----

  document.querySelectorAll("[data-birkan-confirm]").forEach((form) => {
    form.addEventListener("submit", (e) => {
      if (!confirm(form.dataset.birkanConfirm)) e.preventDefault();
    });
  });

  document.querySelectorAll("[data-birkan-notice]").forEach((n) => {
    setTimeout(() => n.classList.add("birkan-is-leaving"), 4200);
  });

  document.querySelector("[data-birkan-sidebar-toggle]")?.addEventListener("click", () => {
    document.body.classList.toggle("birkan-sidebar-open");
  });

  birkanBindImages(document);
  birkanBindAutosize(document);
  birkanRefreshIndexes(document);
})();
