using System.Globalization;
using System.Resources;

namespace Chords.App.Localization;

/// <summary>
/// UI texts from Strings.resx (English) and its translations (Strings.cs.resx, Strings.sk.resx).
/// </summary>
public class Localizer
{
    private static readonly ResourceManager Resources = new("Chords.App.Localization.Strings", typeof(Localizer).Assembly);

    /// <summary>UI culture of the device, captured before the app overrides it.</summary>
    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    /// <summary>Languages offered in settings: culture name and the language's own name.</summary>
    public static readonly (string Code, string Name)[] Languages =
    [
        ("en", "English"),
        ("cs", "Čeština"),
        ("sk", "Slovenčina")
    ];

    /// <summary>Name of the supported language the device language maps to (English when unsupported).</summary>
    public static string SystemLanguageName =>
        Languages.FirstOrDefault(i => i.Code == SystemCulture.TwoLetterISOLanguageName).Name ?? Languages[0].Name;

    /// <summary>Raised after the language changes so already rendered components can refresh.</summary>
    public event Action LanguageChanged;

    public string this[string key] => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public string this[string key, params object[] args] => string.Format(CultureInfo.CurrentCulture, this[key], args);

    /// <summary>
    /// Plural form of a counted text: uses the keys <c>{key}_One</c>, <c>{key}_Few</c> (2–4 in Czech and Slovak)
    /// and <c>{key}_Many</c>, each with a <c>{0}</c> placeholder for the count.
    /// </summary>
    public string Plural(string key, int count)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var form = count == 1 ? "One"
                 : (language is "cs" or "sk") && count is >= 2 and <= 4 ? "Few"
                 : "Many";

        return this[$"{key}_{form}", count];
    }

    /// <summary>Switches the UI language; <c>null</c> follows the device language.</summary>
    public void SetLanguage(string code)
    {
        var culture = string.IsNullOrEmpty(code) ? SystemCulture : CultureInfo.GetCultureInfo(code);

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        LanguageChanged?.Invoke();
    }
}
