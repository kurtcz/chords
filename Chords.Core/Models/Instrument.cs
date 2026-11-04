using System.Linq;

namespace Chords.Core.Models
{
    /// <summary>Stringed instruments the chord diagrams can be generated for.</summary>
    /// <remarks>Values are persisted (settings, favorites) – append new ones, never reorder.</remarks>
    public enum Instrument
    {
        Guitar,
        Ukulele,
        Mandolin,
        BassGuitar,
        Banjo
    }

    /// <summary>Tuning and fingering rules of an <see cref="Instrument"/>.</summary>
    public class InstrumentInfo
    {
        /// <summary>Open strings from the lowest (left in diagrams) to the highest.</summary>
        public Note[] Strings { get; }
        /// <summary>MIDI pitch of each open string (E2 = 40), used for playback.</summary>
        public int[] Pitches { get; }
        /// <summary>Whether some strings may stay silent (only at the edges, never in the middle).</summary>
        public bool AllowMutedStrings { get; }
        /// <summary>
        /// Fret where each string's nut sits: 0 for full-length strings, 5 for the banjo's short 5th string.
        /// A short string can be fretted only beyond its nut; it then sounds <c>fret - FirstFret</c> half-steps above open.
        /// </summary>
        public int[] FirstFrets { get; }

        public int StringCount => Strings.Length;

        private InstrumentInfo(bool allowMutedStrings, params (Tone Tone, int Pitch)[] strings)
        {
            Strings = strings.Select(i => new Note(i.Tone)).ToArray();
            Pitches = strings.Select(i => i.Pitch).ToArray();
            AllowMutedStrings = allowMutedStrings;
            FirstFrets = new int[strings.Length];
        }

        private InstrumentInfo(int[] firstFrets, bool allowMutedStrings, params (Tone Tone, int Pitch)[] strings)
            : this(allowMutedStrings, strings)
        {
            FirstFrets = firstFrets;
        }

        /// <summary>Whether a string is shorter than the neck (banjo's 5th string).</summary>
        public bool IsShortString(int s) => FirstFrets[s] > 0;

        /// <summary>Whether a string can be pressed at a fret (0 = open): a short string only beyond its own nut.</summary>
        public bool CanFret(int s, int fret) => fret <= 0 || fret > FirstFrets[s];

        /// <summary>Half-steps a string sounds above its open note when pressed at a fret of the neck.</summary>
        public int Semitones(int s, int fret) => fret <= 0 ? 0 : fret - FirstFrets[s];

        /// <summary>Note a string sounds when pressed at a fret of the neck (0 = open).</summary>
        public Note NoteAt(int s, int fret) => Strings[s].NoteAtChromaticDistance(Semitones(s, fret));

        /// <summary>
        /// Lowest string a fret wire reaches: on the banjo frets 1–4 lie under four strings only,
        /// from the 5th fret on under all five.
        /// </summary>
        public int FirstStringAtFret(int fret)
        {
            var s = 0;

            while (s < StringCount - 1 && FirstFrets[s] > fret)
            {
                s++;
            }

            return s;
        }

        public static readonly InstrumentInfo Guitar = new InstrumentInfo(true,
            (Tone.E, 40), (Tone.A, 45), (Tone.D, 50), (Tone.G, 55), (Tone.B, 59), (Tone.E, 64));
        /// <summary>Standard re-entrant tuning G4 C4 E4 A4.</summary>
        public static readonly InstrumentInfo Ukulele = new InstrumentInfo(false,
            (Tone.G, 67), (Tone.C, 60), (Tone.E, 64), (Tone.A, 69));
        /// <summary>Four double courses tuned G3 D4 A4 E5.</summary>
        public static readonly InstrumentInfo Mandolin = new InstrumentInfo(false,
            (Tone.G, 55), (Tone.D, 62), (Tone.A, 69), (Tone.E, 76));
        public static readonly InstrumentInfo BassGuitar = new InstrumentInfo(true,
            (Tone.E, 28), (Tone.A, 33), (Tone.D, 38), (Tone.G, 43));
        /// <summary>
        /// Five-string banjo in open G: the short drone string G4 (starting at the 5th fret) on the left, then D3 G3 B3 D4.
        /// </summary>
        public static readonly InstrumentInfo Banjo = new InstrumentInfo(new[] { 5, 0, 0, 0, 0 }, false,
            (Tone.G, 67), (Tone.D, 50), (Tone.G, 55), (Tone.B, 59), (Tone.D, 62));

        public static InstrumentInfo Get(Instrument instrument)
        {
            switch (instrument)
            {
                case Instrument.Ukulele: return Ukulele;
                case Instrument.Mandolin: return Mandolin;
                case Instrument.BassGuitar: return BassGuitar;
                case Instrument.Banjo: return Banjo;
                default: return Guitar;
            }
        }
    }
}
