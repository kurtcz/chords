namespace Chords.Core.Models
{
    /// <summary>How hard a chord fingering is to play, see <see cref="GuitarChordLayout.Difficulty"/>.</summary>
    public enum DifficultyLevel
    {
        /// <summary>Open chords and simple shapes a beginner can play.</summary>
        Easy,
        /// <summary>Barre chords, four-finger shapes, a wider hand.</summary>
        Medium,
        /// <summary>Stretches of the little finger, barre chords with all fingers.</summary>
        Hard
    }
}
