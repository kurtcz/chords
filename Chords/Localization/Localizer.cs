using System.Globalization;
using System.Resources;
using Chords.Core.Models;

namespace Chords.Localization;

/// <summary>
/// UI texts from Strings.resx (English) and its translations (Strings.{culture}.resx).
/// </summary>
/// <remarks>
/// Adding a language: create Strings.{code}.resx with every key of Strings.resx (and the plural forms
/// its <see cref="PluralForm"/> returns), then add the code to <see cref="Languages"/>,
/// <see cref="GermanNotation"/> / <see cref="LatinNotation"/> and, when needed, <see cref="PluralForm"/>.
/// The build declares the language to iOS and Android (PlatformLocales.targets), which needs its Tuner_MicUsage.
/// </remarks>
public class Localizer
{
    private static readonly ResourceManager Resources = new("Chords.Localization.Strings", typeof(Localizer).Assembly);

    /// <summary>UI culture of the device, captured before the app overrides it.</summary>
    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    /// <summary>
    /// Languages offered in settings: culture name, the language's own name and a flag emoji shown before it
    /// (British for English, Spanish for Spanish, Andorran for Catalan, which has no flag emoji of its own).
    /// </summary>
    public static readonly (string Code, string Name, string Flag)[] Languages =
    [
        ("en", "English", "🇬🇧"),
        ("az", "Azərbaycan", "🇦🇿"),
        ("id", "Bahasa Indonesia", "🇮🇩"),
        ("ms", "Bahasa Melayu", "🇲🇾"),
        ("bs", "Bosanski", "🇧🇦"),
        ("ca", "Català", "🇦🇩"),
        ("cs", "Čeština", "🇨🇿"),
        ("cy", "Cymraeg", "🏴󠁧󠁢󠁷󠁬󠁳󠁿"),
        ("da", "Dansk", "🇩🇰"),
        ("de", "Deutsch", "🇩🇪"),
        ("et", "Eesti", "🇪🇪"),
        ("es", "Español", "🇪🇸"),
        ("eu", "Euskara", "🇪🇸"),
        ("fil", "Filipino", "🇵🇭"),
        ("fr", "Français", "🇫🇷"),
        ("ga", "Gaeilge", "🇮🇪"),
        ("gl", "Galego", "🇪🇸"),
        ("hr", "Hrvatski", "🇭🇷"),
        ("is", "Íslenska", "🇮🇸"),
        ("it", "Italiano", "🇮🇹"),
        ("lv", "Latviešu", "🇱🇻"),
        ("lb", "Lëtzebuergesch", "🇱🇺"),
        ("lt", "Lietuvių", "🇱🇹"),
        ("hu", "Magyar", "🇭🇺"),
        ("mt", "Malti", "🇲🇹"),
        ("nl", "Nederlands", "🇳🇱"),
        ("nb", "Norsk", "🇳🇴"),
        ("uz", "Oʻzbek", "🇺🇿"),
        ("pl", "Polski", "🇵🇱"),
        ("pt", "Português (Portugal)", "🇵🇹"),
        ("pt-BR", "Português (Brasil)", "🇧🇷"),
        ("ro", "Română", "🇷🇴"),
        ("sq", "Shqip", "🇦🇱"),
        ("sk", "Slovenčina", "🇸🇰"),
        ("sl", "Slovenščina", "🇸🇮"),
        ("fi", "Suomi", "🇫🇮"),
        ("sv", "Svenska", "🇸🇪"),
        ("vi", "Tiếng Việt", "🇻🇳"),
        ("tr", "Türkçe", "🇹🇷"),
        ("el", "Ελληνικά", "🇬🇷"),
        ("be", "Беларуская", "🇧🇾"),
        ("bg", "Български", "🇧🇬"),
        ("kk", "Қазақ", "🇰🇿"),
        ("mk", "Македонски", "🇲🇰"),
        ("mn", "Монгол", "🇲🇳"),
        ("ru", "Русский", "🇷🇺"),
        ("sr", "Српски", "🇷🇸"),
        ("uk", "Українська", "🇺🇦"),
        ("hy", "Հայերեն", "🇦🇲"),
        ("ka", "ქართული", "🇬🇪"),
        ("he", "עברית", "🇮🇱"),
        ("ar", "العربية", "🇸🇦"),
        ("fa", "فارسی", "🇮🇷"),
        ("ur", "اردو", "🇵🇰"),
        ("hi", "हिन्दी", "🇮🇳"),
        ("mr", "मराठी", "🇮🇳"),
        ("bn", "বাংলা", "🇧🇩"),
        ("ta", "தமிழ்", "🇮🇳"),
        ("te", "తెలుగు", "🇮🇳"),
        ("th", "ไทย", "🇹🇭"),
        ("ko", "한국어", "🇰🇷"),
        ("ja", "日本語", "🇯🇵"),
        ("zh-Hans", "简体中文", "🇨🇳"),
        ("zh-Hant", "繁體中文", "🇹🇼")
    ];

    /// <summary>Languages whose musicians name B natural "H" (C D E F G A H).</summary>
    private static readonly HashSet<string> GermanNotation =
    [
        "de", "lb", "cs", "sk", "pl", "hu", "sl", "hr", "bs", "sr", "mk",
        "da", "nb", "sv", "fi", "et", "lv", "lt", "is", "ru", "uk", "be"
    ];

    /// <summary>Languages that name notes by solfège (Do Re Mi Fa Sol La Si).</summary>
    private static readonly HashSet<string> LatinNotation =
    [
        "es", "pt", "pt-BR", "fr", "it", "ro", "ca", "gl", "eu", "mt",
        "el", "bg", "sq", "tr", "ar", "fa"
    ];

    private static readonly HashSet<string> RightToLeft = ["ar", "fa", "he", "ur"];

    /// <summary>Supported language the device language maps to (English when unsupported).</summary>
    public static string SystemLanguage => Resolve(SystemCulture);

    /// <summary>Supported language the UI is shown in.</summary>
    public static string CurrentLanguage => Resolve(CultureInfo.CurrentUICulture);

    /// <summary>Whether the UI is laid out right to left (Arabic, Persian, Hebrew, Urdu).</summary>
    public static bool IsRightToLeft => RightToLeft.Contains(CurrentLanguage);

    /// <summary>Note naming usual for a language: H in central and northern Europe, Do Re Mi in Romance languages.</summary>
    public static NamingConvention DefaultNotation(string language) =>
        GermanNotation.Contains(language) ? NamingConvention.German
        : LatinNotation.Contains(language) ? NamingConvention.Latin
        : NamingConvention.English;

    /// <summary>Raised after the language changes so already rendered components can refresh.</summary>
    public event Action LanguageChanged;

    public string this[string key] => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    /// <summary>Name of a tuning: translated for presets like "Standard tuning", as given otherwise.</summary>
    public string TuningName(InstrumentTuning tuning)
        => tuning.NameKey != null ? this[tuning.NameKey] : string.IsNullOrEmpty(tuning.Name) ? this["Tuning_Custom"] : tuning.Name;

    /// <summary>Open strings from the lowest, e.g. "D-A-D-G-B-E".</summary>
    public static string TuningNotes(InstrumentTuning tuning, NamingConvention conv, string separator = "-")
        => string.Join(separator, Enumerable.Range(0, tuning.Pitches.Length).Select(s => tuning.NoteOf(s).ToString(conv)));

    public string this[string key, params object[] args] => string.Format(CultureInfo.CurrentCulture, this[key], args);

    /// <summary>
    /// Plural form of a counted text: uses the key <c>{key}_{form}</c> with a <c>{0}</c> placeholder for the count,
    /// where form is Zero, One, Two, Few, Many or Other as the language's grammar requires (see <see cref="PluralForm"/>).
    /// Many is the general plural every language defines; the other forms fall back to it.
    /// </summary>
    public string Plural(string key, int count)
    {
        var form = PluralForm(CurrentLanguage, count);

        return Resources.GetString($"{key}_{form}", CultureInfo.CurrentUICulture) is { } text
            ? string.Format(CultureInfo.CurrentCulture, text, count)
            : this[$"{key}_Many", count];
    }

    /// <summary>Plural category of a whole number, after the CLDR plural rules.</summary>
    public static string PluralForm(string language, int count)
    {
        var n10 = count % 10;
        var n100 = count % 100;

        switch (language)
        {
            case "ja":
            case "zh-Hans":
            case "zh-Hant":
            case "ko":
            case "vi":
            case "th":
            case "id":
            case "ms":
                return "Many";
            case "fr":
            case "pt-BR":
            case "fa":
            case "hi":
            case "bn":
            case "hy":
                return count is 0 or 1 ? "One" : "Many";
            case "cs":
            case "sk":
                return count == 1 ? "One" : count is >= 2 and <= 4 ? "Few" : "Many";
            case "pl":
                return count == 1 ? "One"
                     : n10 is >= 2 and <= 4 && n100 is < 12 or > 14 ? "Few"
                     : "Many";
            case "ru":
            case "uk":
            case "be":
            case "hr":
            case "bs":
            case "sr":
                return n10 == 1 && n100 != 11 ? "One"
                     : n10 is >= 2 and <= 4 && n100 is < 12 or > 14 ? "Few"
                     : "Many";
            case "he":
                return count == 1 ? "One" : count == 2 ? "Two" : "Many";
            case "fil":
                return n10 is 4 or 6 or 9 ? "Many" : "One";
            case "sl":
                return n100 == 1 ? "One" : n100 == 2 ? "Two" : n100 is 3 or 4 ? "Few" : "Many";
            case "lt":
                return n100 is >= 11 and <= 19 ? "Many"
                     : n10 == 1 ? "One"
                     : n10 >= 2 ? "Few"
                     : "Many";
            case "lv":
                return n10 == 0 || n100 is >= 11 and <= 19 ? "Zero" : n10 == 1 ? "One" : "Many";
            case "ro":
                return count == 1 ? "One" : count == 0 || n100 is >= 2 and <= 19 ? "Few" : "Many";
            case "is":
            case "mk":
                return n10 == 1 && n100 != 11 ? "One" : "Many";
            case "ga":
                return count == 1 ? "One" : count == 2 ? "Two" : count is >= 3 and <= 6 ? "Few"
                     : count is >= 7 and <= 10 ? "Many" : "Other";
            case "mt":
                return count == 1 ? "One" : count == 2 ? "Two" : count == 0 || n100 is >= 3 and <= 10 ? "Few"
                     : n100 is >= 11 and <= 19 ? "Many" : "Other";
            case "ar":
                return count == 0 ? "Zero" : count == 1 ? "One" : count == 2 ? "Two"
                     : n100 is >= 3 and <= 10 ? "Few" : n100 >= 11 ? "Many" : "Other";
            case "cy":
                return count switch { 0 => "Zero", 1 => "One", 2 => "Two", 3 => "Few", 6 => "Many", _ => "Other" };
            default:
                return count == 1 ? "One" : "Many";
        }
    }

    /// <summary>Switches the UI language; <c>null</c> follows the device language.</summary>
    public void SetLanguage(string code)
    {
        var culture = string.IsNullOrEmpty(code) ? SystemCulture : CultureInfo.GetCultureInfo(code);

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        LanguageChanged?.Invoke();
    }

    /// <summary>Most specific supported language of a culture (zh-TW → zh-Hant, pt-BR → pt-BR, de-AT → de).</summary>
    private static string Resolve(CultureInfo culture)
    {
        for (var c = culture; !string.IsNullOrEmpty(c.Name); c = c.Parent)
        {
            var match = Languages.FirstOrDefault(i => string.Equals(i.Code, c.Name, StringComparison.OrdinalIgnoreCase)).Code;

            if (match != null)
            {
                return match;
            }
            // Norwegian: "no" and "nn" devices get Bokmål.
            if (c.TwoLetterISOLanguageName is "no" or "nn")
            {
                return "nb";
            }
        }
        return "en";
    }
}
