using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, InstrumentInfo> _tuned = new ConcurrentDictionary<string, InstrumentInfo>();

        public Instrument Instrument { get; private set; }
        /// <summary>Whether the strings are tuned the usual way (not to an alternate <see cref="InstrumentTuning"/>).</summary>
        public bool IsStandardTuning => ReferenceEquals(this, Get(Instrument));
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
        /// <summary>Frets the fretting hand covers with one finger per fret, without stretching.</summary>
        public int ComfortableFretSpan { get; private set; } = 4;
        /// <summary>
        /// Widest span reachable by stretching the little finger alone to the farthest fret
        /// (equal to <see cref="ComfortableFretSpan"/> when the frets are too wide to stretch).
        /// </summary>
        public int MaxFretSpan { get; private set; } = 4;
        /// <summary>
        /// Whether a string in the middle may stay silent: only when the strings are plucked one by one
        /// (fingerstyle bass), never when strummed.
        /// </summary>
        public bool AllowInnerMutedStrings { get; private set; }
        /// <summary>Frets shown in a chord diagram, enough for the widest chord.</summary>
        public int DiagramFrets => Math.Max(4, MaxFretSpan);

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

        /// <summary>The same instrument with its strings tuned to other pitches.</summary>
        private InstrumentInfo(InstrumentInfo standard, int[] pitches)
        {
            Strings = pitches.Select(p => Note.FromPitch(p)).ToArray();
            Pitches = pitches.ToArray();
            AllowMutedStrings = standard.AllowMutedStrings;
            FirstFrets = standard.FirstFrets;
            ComfortableFretSpan = standard.ComfortableFretSpan;
            MaxFretSpan = standard.MaxFretSpan;
            AllowInnerMutedStrings = standard.AllowInnerMutedStrings;
            Instrument = standard.Instrument;
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
            (Tone.E, 40), (Tone.A, 45), (Tone.D, 50), (Tone.G, 55), (Tone.B, 59), (Tone.E, 64))
        {
            Instrument = Instrument.Guitar
        };
        /// <summary>
        /// Standard re-entrant tuning G4 C4 E4 A4, strummed. The short scale (33–38 cm) lets the little finger
        /// stretch one fret beyond the usual four.
        /// </summary>
        public static readonly InstrumentInfo Ukulele = new InstrumentInfo(false,
            (Tone.G, 67), (Tone.C, 60), (Tone.E, 64), (Tone.A, 69))
        {
            Instrument = Instrument.Ukulele,
            ComfortableFretSpan = 4,
            MaxFretSpan = 5
        };
        /// <summary>
        /// Four double courses tuned G3 D4 A4 E5, strummed. Tuned in fifths like a violin, with a 35 cm scale whose
        /// frets are about half as wide as a guitar's: closed shapes span five frets and the little finger
        /// reaches a sixth (e.g. the G chop chord 7 5 2 3).
        /// </summary>
        public static readonly InstrumentInfo Mandolin = new InstrumentInfo(false,
            (Tone.G, 55), (Tone.D, 62), (Tone.A, 69), (Tone.E, 76))
        {
            Instrument = Instrument.Mandolin,
            ComfortableFretSpan = 5,
            MaxFretSpan = 6
        };
        /// <summary>
        /// Plucked with the fingers, so any string may stay silent. The long scale (86 cm) has wide frets:
        /// three under the hand, the fourth only with a stretch of the little finger.
        /// </summary>
        public static readonly InstrumentInfo BassGuitar = new InstrumentInfo(true,
            (Tone.E, 28), (Tone.A, 33), (Tone.D, 38), (Tone.G, 43))
        {
            Instrument = Instrument.BassGuitar,
            ComfortableFretSpan = 3,
            MaxFretSpan = 4,
            AllowInnerMutedStrings = true
        };
        /// <summary>
        /// Five-string banjo in open G: the short drone string G4 (starting at the 5th fret) on the left, then D3 G3 B3 D4.
        /// Its scale is about the guitar's, so is the reach: four frets.
        /// </summary>
        public static readonly InstrumentInfo Banjo = new InstrumentInfo(new[] { 5, 0, 0, 0, 0 }, false,
            (Tone.G, 67), (Tone.D, 50), (Tone.G, 55), (Tone.B, 59), (Tone.D, 62))
        {
            Instrument = Instrument.Banjo
        };

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

        /// <summary>
        /// The instrument with its open strings tuned to the given MIDI pitches (lowest string first);
        /// the standard one for null, standard pitches or a wrong number of strings.
        /// </summary>
        /// <remarks>Instances are cached: one tuning is always the same instance.</remarks>
        public static InstrumentInfo Get(Instrument instrument, int[] pitches)
        {
            var standard = Get(instrument);

            if (pitches == null || pitches.Length != standard.StringCount || pitches.SequenceEqual(standard.Pitches))
            {
                return standard;
            }

            return _tuned.GetOrAdd($"{instrument}:{string.Join(",", pitches)}", _ => new InstrumentInfo(standard, pitches));
        }
    }
}
