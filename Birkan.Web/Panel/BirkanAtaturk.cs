namespace Birkan.Web.Panel;

// Panelde gösterilen Atatürk sözleri ve kronolojisi.
public static class BirkanAtaturk
{
    public static readonly string[] Quotes =
    [
        "Hayatta en hakiki mürşit ilimdir.",
        "Yurtta sulh, cihanda sulh.",
        "Egemenlik kayıtsız şartsız milletindir.",
        "Ne mutlu Türk'üm diyene!",
        "Muhtaç olduğun kudret, damarlarındaki asil kanda mevcuttur.",
        "Benim naçiz vücudum elbet bir gün toprak olacaktır, ancak Türkiye Cumhuriyeti ilelebet payidar kalacaktır.",
        "Öğretmenler, yeni nesil sizin eseriniz olacaktır.",
        "Sanatsız kalan bir milletin hayat damarlarından biri kopmuş olur.",
        "Biz her şeyi gençliğe bırakacağız."
    ];

    public static readonly (string Date, string Text)[] Chronology =
    [
        ("1881", "Selanik'te doğdu."),
        ("1915", "Çanakkale Cephesi'nde Anafartalar Grup Komutanı."),
        ("19 Mayıs 1919", "Samsun'a çıkışıyla Millî Mücadele başladı."),
        ("23 Nisan 1920", "Türkiye Büyük Millet Meclisi açıldı."),
        ("30 Ağustos 1922", "Başkomutanlık Meydan Muharebesi kazanıldı."),
        ("29 Ekim 1923", "Cumhuriyet ilan edildi; ilk Cumhurbaşkanı seçildi."),
        ("1 Kasım 1928", "Yeni Türk harfleri kabul edildi."),
        ("5 Aralık 1934", "Kadınlara milletvekili seçme ve seçilme hakkı tanındı."),
        ("10 Kasım 1938", "Dolmabahçe Sarayı'nda hayata gözlerini yumdu.")
    ];

    public static int RepublicYear => DateTime.Now.Year - 1923;

    // Her gün farklı bir söz gösterilsin; gün içinde sayfa yenilense de aynı kalsın.
    public static string QuoteOfTheDay => Quotes[DateTime.Now.DayOfYear % Quotes.Length];
}
