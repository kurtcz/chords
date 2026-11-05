using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Chords.Core.Models
{
    /// <summary>
    /// Pitches the open strings of an <see cref="Instrument"/> are tuned to: a common alternate tuning
    /// (<see cref="Presets"/>) or one the player defines.
    /// </summary>
    public class InstrumentTuning
    {
        /// <summary>Name of a custom tuning, or of a preset known by the same name everywhere ("Drop D", "DADGAD").</summary>
        public string Name { get; }
        /// <summary>Localization key of a preset whose name is translated ("Tuning_Standard"); null otherwise.</summary>
        public string NameKey { get; }
        public Instrument Instrument { get; }
        /// <summary>MIDI pitch of each open string, from the lowest (left in diagrams) like <see cref="InstrumentInfo.Pitches"/>.</summary>
        public int[] Pitches { get; }
        /// <summary>Spell the black keys as flats (E♭ tuning) instead of sharps.</summary>
        public bool Flats { get; }

        [JsonConstructor]
        public InstrumentTuning(string name, Instrument instrument, int[] pitches, string nameKey = null, bool? flats = null)
        {
            Name = name;
            NameKey = nameKey;
            Instrument = instrument;
            Pitches = pitches;
            //a custom tuning with strings only tuned down (E -> Eb, half step down) reads better in flats
            var standard = Standard(instrument);
            Flats = flats ?? (pitches.Length == standard.Length &&
                              pitches.Zip(standard, (p, s) => p < s).Any(i => i) &&
                              !pitches.Zip(standard, (p, s) => p > s).Any(i => i));
        }

        [JsonIgnore]
        public bool IsStandard => SamePitches(Pitches, Standard(Instrument));

        /// <summary>Half-steps the string is tuned above (+) or below (−) the standard tuning.</summary>
        public int Offset(int s) => Pitches[s] - Standard(Instrument)[s];

        /// <summary>Note of an open string, spelled the way this tuning is usually written.</summary>
        public Note NoteOf(int s) => Note.FromPitch(Pitches[s], Flats);

        public InstrumentInfo Info => InstrumentInfo.Get(Instrument, Pitches);

        public static int[] Standard(Instrument instrument) => InstrumentInfo.Get(instrument).Pitches;

        public static bool SamePitches(int[] a, int[] b) => a != null && b != null && a.SequenceEqual(b);

        private static InstrumentTuning Std(Instrument i) => new InstrumentTuning(null, i, Standard(i), "Tuning_Standard", false);

        private static readonly Dictionary<Instrument, InstrumentTuning[]> _presets = new Dictionary<Instrument, InstrumentTuning[]>
        {
            [Instrument.Guitar] = new[]
            {
                Std(Instrument.Guitar),
                new InstrumentTuning("Drop D", Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 }),
                new InstrumentTuning(null, Instrument.Guitar, new[] { 39, 44, 49, 54, 58, 63 }, "Tuning_HalfStepDown", true),
                new InstrumentTuning(null, Instrument.Guitar, new[] { 38, 43, 48, 53, 57, 62 }, "Tuning_WholeStepDown", false),
                new InstrumentTuning("Drop C", Instrument.Guitar, new[] { 36, 43, 48, 53, 57, 62 }),
                new InstrumentTuning("Double Drop D", Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 62 }),
                new InstrumentTuning("DADGAD", Instrument.Guitar, new[] { 38, 45, 50, 55, 57, 62 }),
                new InstrumentTuning("Open D", Instrument.Guitar, new[] { 38, 45, 50, 54, 57, 62 }, flats: false),
                new InstrumentTuning("Open G", Instrument.Guitar, new[] { 38, 43, 50, 55, 59, 62 }),
                new InstrumentTuning("Open E", Instrument.Guitar, new[] { 40, 47, 52, 56, 59, 64 }),
                new InstrumentTuning("Open C", Instrument.Guitar, new[] { 36, 43, 48, 55, 60, 64 }),
            },
            [Instrument.Ukulele] = new[]
            {
                Std(Instrument.Ukulele),
                new InstrumentTuning(null, Instrument.Ukulele, new[] { 55, 60, 64, 69 }, "Tuning_LowG"),
                new InstrumentTuning(null, Instrument.Ukulele, new[] { 69, 62, 66, 71 }, "Tuning_UkuleleD", false),
                new InstrumentTuning(null, Instrument.Ukulele, new[] { 50, 55, 59, 64 }, "Tuning_Baritone"),
            },
            [Instrument.Mandolin] = new[]
            {
                Std(Instrument.Mandolin),
                new InstrumentTuning("AEAE", Instrument.Mandolin, new[] { 57, 64, 69, 76 }),
                new InstrumentTuning("ADAE", Instrument.Mandolin, new[] { 57, 62, 69, 76 }),
                new InstrumentTuning("GDGD", Instrument.Mandolin, new[] { 55, 62, 67, 74 }),
            },
            [Instrument.BassGuitar] = new[]
            {
                Std(Instrument.BassGuitar),
                new InstrumentTuning("Drop D", Instrument.BassGuitar, new[] { 26, 33, 38, 43 }),
                new InstrumentTuning(null, Instrument.BassGuitar, new[] { 27, 32, 37, 42 }, "Tuning_HalfStepDown", true),
                new InstrumentTuning(null, Instrument.BassGuitar, new[] { 26, 31, 36, 41 }, "Tuning_WholeStepDown", false),
                new InstrumentTuning("Drop C", Instrument.BassGuitar, new[] { 24, 31, 36, 41 }),
                new InstrumentTuning(null, Instrument.BassGuitar, new[] { 33, 38, 43, 48 }, "Tuning_Tenor"),
            },
            [Instrument.Banjo] = new[]
            {
                Std(Instrument.Banjo),
                new InstrumentTuning("Double C", Instrument.Banjo, new[] { 67, 48, 55, 60, 62 }),
                new InstrumentTuning("Open D", Instrument.Banjo, new[] { 66, 50, 54, 57, 62 }, flats: false),
                new InstrumentTuning("Sawmill (G modal)", Instrument.Banjo, new[] { 67, 50, 55, 60, 62 }),
                new InstrumentTuning("Open C", Instrument.Banjo, new[] { 67, 48, 55, 59, 62 }),
            },
        };

        /// <summary>Standard and the usual alternate tunings of an instrument, standard first.</summary>
        public static IReadOnlyList<InstrumentTuning> Presets(Instrument instrument) => _presets[instrument];

        /// <summary>Preset with these pitches, or null.</summary>
        public static InstrumentTuning FindPreset(Instrument instrument, int[] pitches)
            => _presets[instrument].FirstOrDefault(i => SamePitches(i.Pitches, pitches));
    }
}
