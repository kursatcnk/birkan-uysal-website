using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Birkan.Web.Services;

public static partial class BirkanSafe
{
    // Görsel yolları yalnızca sitenin kendi medya klasöründen olabilir; CSS ya da HTML içine kaçış yapılamasın.
    public static string MediaPath(string? path) =>
        !string.IsNullOrEmpty(path) && MediaPathPattern().IsMatch(path) ? path : "";

    // Bağlantılarda javascript: gibi şemalar engelleniyor.
    public static string Url(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        url = url.Trim();
        return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith('#')
            ? url
            : "";
    }

    // Formdan gelen boş alanlar null olarak bağlanıyor. Kaydetmeden önce metinler "" yapılıyor,
    // listelerdeki boş satırlar atılıyor; böylece JSON ve site hep aynı biçimde kalıyor.
    public static T Normalize<T>(T model) where T : class
    {
        NormalizeObject(model);
        return model;
    }

    private static void NormalizeObject(object? obj)
    {
        if (obj is null) return;

        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite || prop.GetIndexParameters().Length > 0) continue;
            var type = prop.PropertyType;

            if (type == typeof(string))
            {
                prop.SetValue(obj, ((string?)prop.GetValue(obj))?.Trim() ?? "");
            }
            else if (type == typeof(List<string>))
            {
                var list = (List<string>?)prop.GetValue(obj) ?? [];
                prop.SetValue(obj, list.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList());
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (IList?)prop.GetValue(obj);
                if (list is null)
                {
                    prop.SetValue(obj, Activator.CreateInstance(type));
                    continue;
                }
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] is null) list.RemoveAt(i);
                    else NormalizeObject(list[i]);
                }
            }
            else if (type.IsClass && type.Namespace?.StartsWith("Birkan.Web.Models") == true)
            {
                var value = prop.GetValue(obj) ?? Activator.CreateInstance(type);
                NormalizeObject(value);
                prop.SetValue(obj, value);
            }
        }
    }

    [GeneratedRegex(@"^/birkan-media/[A-Za-z0-9/_\-.]+$")]
    private static partial Regex MediaPathPattern();
}
