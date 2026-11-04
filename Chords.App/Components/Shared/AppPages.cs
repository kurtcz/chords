namespace Chords.App.Components.Shared;

/// <summary>Tab of the bottom bar a page belongs to.</summary>
public enum AppSection
{
    Chords,
    Favorites,
    Theory,
    Tuner,
    More,
}

/// <summary>A page reachable from the navigation: its route, the resource key of its title, its icon and its tab.</summary>
public sealed record AppPage(string Href, string TitleKey, string Icon, AppSection Section);

/// <summary>Every page of the app in the order and groups the "More" menu lists them.</summary>
public static class AppPages
{
    public static readonly AppPage ShowChord = new("", "Nav_ShowChord", "chord", AppSection.Chords);
    public static readonly AppPage ChordDetail = new("chord", "Nav_ShowChord", "chord", AppSection.Chords);
    public static readonly AppPage FindChord = new("find", "Nav_FindChord", "search", AppSection.Chords);
    public static readonly AppPage FindShape = new("shape", "Nav_FindShape", "tap", AppSection.Chords);
    public static readonly AppPage Favorites = new("favorites", "Nav_Favorites", "star", AppSection.Favorites);
    public static readonly AppPage Key = new("key", "Nav_Key", "scale", AppSection.Theory);
    public static readonly AppPage Circle = new("circle", "Nav_Circle", "circle", AppSection.Theory);
    public static readonly AppPage Progressions = new("progressions", "Nav_Progressions", "progression", AppSection.Theory);
    public static readonly AppPage Tuner = new("tuner", "Nav_Tuner", "tuner", AppSection.Tuner);
    public static readonly AppPage Settings = new("settings", "Nav_Settings", "settings", AppSection.More);

    /// <summary>Groups of the "More" menu: resource key of the heading (null: no heading) and its pages.</summary>
    public static readonly (string HeadingKey, AppPage[] Pages)[] Groups =
    [
        ("Tab_Chords", [ShowChord, FindChord, FindShape, Favorites]),
        ("Tab_Theory", [Key, Circle, Progressions]),
        ("Group_Tools", [Tuner]),
        (null, [Settings]),
    ];

    private static readonly AppPage[] All = [ShowChord, ChordDetail, FindChord, FindShape, Favorites, Key, Circle, Progressions, Tuner, Settings];

    /// <summary>The page of a relative URI ("key?tab=chords", "chord?root=C"); null for an unknown one.</summary>
    public static AppPage FromRelativeUri(string relative)
    {
        var path = relative.Split('?', '#')[0].Trim('/');
        return All.FirstOrDefault(i => string.Equals(i.Href, path, StringComparison.OrdinalIgnoreCase));
    }
}
