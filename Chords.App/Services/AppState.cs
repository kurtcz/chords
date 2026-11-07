using Chords.App.Localization;
using Chords.App.Models;
using Chords.Core.Models;

namespace Chords.App.Services;

/// <summary>State shared by all pages for the lifetime of the app.</summary>
public class AppState
{
    public SettingsModel Settings { get; } = SettingsModel.Load();

    public AppState(Localizer localizer)
    {
        localizer.SetLanguage(Settings.Language);
        if (Settings.IsNew)
        {
            // First run: name notes the way musicians of the device language do.
            Settings.conv = Localizer.DefaultNotation(Localizer.CurrentLanguage);
        }
    }

    /// <summary>Chord last shown on the chord page; preselected when returning to "Show chord chart".</summary>
    public Note CurrentRoot { get; set; }
    public ChordType CurrentChordType { get; set; }

    /// <summary>Strict mode on the "Find chord name" page.</summary>
    public bool StrictFind { get; set; }

    /// <summary>Notes selected on the "Find chord name" page, kept while navigating.</summary>
    public HashSet<string> FindNotes { get; } = [];
    public NamingConvention FindNotesConvention { get; set; }

    /// <summary>Fret held on each string on the "Find chord by shape" page (0 = open, -1 = muted).</summary>
    public int[] ShapeFrets { get; set; }
    public Instrument ShapeInstrument { get; set; }

    /// <summary>
    /// Key chosen on the "Chords in a key" and "Chord progressions" pages; the circle of fifths shows its
    /// parent major key.
    /// </summary>
    public Note KeyTonic { get; set; } = new(Tone.C);
    public ScaleType KeyScale { get; set; } = ScaleType.Major;
    /// <summary>Seventh chords instead of triads in a key and its progressions.</summary>
    public bool KeySevenths { get; set; }

    /// <summary>Chords entered to find the key of a song.</summary>
    public List<Chord> KeyDetectChords { get; } = [];

    /// <summary>Theory page visited last (key, circle or progressions); the Theory tab goes back to it.</summary>
    public string TheoryHref { get; set; } = "key";
}
