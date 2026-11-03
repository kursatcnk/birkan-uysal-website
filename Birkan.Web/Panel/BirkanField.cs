using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Birkan.Web.Panel;

// Panel formlarındaki alanlar tek tip görünsün diye ortak HTML üreticisi.
// "rel" ile işaretlenen alanlar tekrarlanan bir grubun içinde; adları gönderimden hemen önce birkan-panel.js tarafından
// Items[2].School gibi tam adlara çevriliyor. Böylece öğe eklenip silindiğinde ya da sırası değiştiğinde numaralar hep doğru kalıyor.
public static class BirkanField
{
    private static readonly HtmlEncoder Enc = HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All);

    public static IHtmlContent Text(string label, string name, string? value, bool rel = false, string? hint = null, string type = "text", string? placeholder = null) =>
        Wrap(label, hint,
            $"<input class=\"birkan-input\" type=\"{type}\" {NameAttr(name, rel)} value=\"{Enc.Encode(value ?? "")}\"{Placeholder(placeholder)} />");

    public static IHtmlContent Area(string label, string name, string? value, bool rel = false, int rows = 4, string? hint = null) =>
        Wrap(label, hint,
            $"<textarea class=\"birkan-input birkan-textarea\" rows=\"{rows}\" {NameAttr(name, rel)} data-birkan-autosize>{Enc.Encode(value ?? "")}</textarea>");

    // Görsel alanı: değer gizli alanda yol olarak tutuluyor, dosya seçilince hemen yükleniyor ve önizleme yenileniyor.
    public static IHtmlContent Image(string label, string name, string? value, string kind, bool rel = false, string? hint = null)
    {
        var v = Enc.Encode(value ?? "");
        var preview = string.IsNullOrEmpty(value)
            ? "<span class=\"birkan-image-empty\">Görsel yok</span>"
            : $"<img src=\"{v}\" alt=\"\" />";
        return Wrap(label, hint, $"""
            <div class="birkan-image-field" data-birkan-image data-kind="{kind}">
              <div class="birkan-image-preview birkan-image-preview-{kind}">{preview}</div>
              <input type="hidden" {NameAttr(name, rel)} value="{v}" data-birkan-image-value />
              <div class="birkan-image-actions">
                <label class="birkan-btn birkan-btn-ghost birkan-btn-sm">
                  <input type="file" accept="image/jpeg,image/png,image/webp" hidden data-birkan-image-file />
                  Görsel seç
                </label>
                <button type="button" class="birkan-btn birkan-btn-link birkan-btn-sm" data-birkan-image-clear>Kaldır</button>
                <span class="birkan-image-status" data-birkan-image-status></span>
              </div>
            </div>
            """, tag: "div");
    }

    private static string NameAttr(string name, bool rel) =>
        rel ? $"data-birkan-name=\"{Enc.Encode(name)}\"" : $"name=\"{Enc.Encode(name)}\"";

    private static string Placeholder(string? p) =>
        string.IsNullOrEmpty(p) ? "" : $" placeholder=\"{Enc.Encode(p)}\"";

    // Görsel alanının içinde dosya seçici etiketi olduğu için o alan <label> yerine <div> ile sarılıyor.
    private static HtmlString Wrap(string label, string? hint, string control, string tag = "label") =>
        new($"""
            <{tag} class="birkan-field">
              <span class="birkan-field-label">{Enc.Encode(label)}</span>
              {control}
              {(string.IsNullOrEmpty(hint) ? "" : $"<span class=\"birkan-field-hint\">{Enc.Encode(hint)}</span>")}
            </{tag}>
            """);
}
