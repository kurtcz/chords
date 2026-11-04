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
        BassGuitar
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

        public int StringCount => Strings.Length;

        private InstrumentInfo(bool allowMutedStrings, params (Tone Tone, int Pitch)[] strings)
        {
            Strings = strings.Select(i => new Note(i.Tone)).ToArray();
            Pitches = strings.Select(i => i.Pitch).ToArray();
            AllowMutedStrings = allowMutedStrings;
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

        public static InstrumentInfo Get(Instrument instrument)
        {
            switch (instrument)
            {
                case Instrument.Ukulele: return Ukulele;
                case Instrument.Mandolin: return Mandolin;
                case Instrument.BassGuitar: return BassGuitar;
                default: return Guitar;
            }
        }
    }
}
