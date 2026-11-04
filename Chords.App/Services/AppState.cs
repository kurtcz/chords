using Chords.App.Models;
using Chords.Core.Models;

namespace Chords.App.Services;

/// <summary>State shared by all pages for the lifetime of the app.</summary>
public class AppState
{
    public SettingsModel Settings { get; } = SettingsModel.Load();

    /// <summary>Chord last shown on the chord page; preselected when returning to "Show chord chart".</summary>
    public Note CurrentRoot { get; set; }
    public ChordType CurrentChordType { get; set; }

    /// <summary>Strict mode on the "Find chord name" page.</summary>
    public bool StrictFind { get; set; }

    /// <summary>Key shown on the circle of fifths (13 = C major).</summary>
    public int CircleIndex { get; set; } = 13;

    /// <summary>Notes selected on the "Find chord name" page, kept while navigating.</summary>
    public HashSet<string> FindNotes { get; } = [];
    public NamingConvention FindNotesConvention { get; set; }
}
