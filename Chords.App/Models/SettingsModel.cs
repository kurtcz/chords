using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chords.Core.Models;
using Newtonsoft.Json;

namespace Chords.App.Models
{
    public class SettingsModel
    {
		public static string FilePath = AppPaths.File("settings.json");
		public NamingConvention conv { get; set; } = NamingConvention.English;
        public int maxFret { get; set; } = 10;
        public bool ShowBasicChordTypes { get; set; } = true;
        public bool AllowBarre { get; set; } = true;
        public bool AllowSpecial { get; set; } = false;
        /// <summary>Show only easy fingerings (or the easiest ones there are), see <see cref="GuitarChordLayout.DifficultyLevel"/>.</summary>
        public bool EasyOnly { get; set; } = false;
        public bool ShowTips { get; set; } = true;
        public Instrument Instrument { get; set; } = Instrument.Guitar;
        /// <summary>Tuning chosen for each instrument: MIDI pitches of the open strings; missing = standard tuning.</summary>
        public Dictionary<Instrument, int[]> Tunings { get; set; } = new Dictionary<Instrument, int[]>();
        /// <summary>Fret the capo is on for each instrument; missing = no capo.</summary>
        public Dictionary<Instrument, int> Capos { get; set; } = new Dictionary<Instrument, int>();
        /// <summary>Tunings the player has defined and named, of all instruments.</summary>
        public List<InstrumentTuning> CustomTunings { get; set; } = new List<InstrumentTuning>();
        /// <summary>Draw chord diagrams with horizontal strings (nut on the left), like looking at the neck.</summary>
        public bool Horizontal { get; set; } = false;
        /// <summary>Mirror horizontal chord diagrams so the nut is on the right.</summary>
        public bool LeftHanded { get; set; } = false;
        /// <summary>Concert pitch the tuner tunes to: frequency of A4 in Hz.</summary>
        public int TunerA4 { get; set; } = 440;
        /// <summary>The tuner shows the nearest semitone instead of the nearest string of the instrument.</summary>
        public bool TunerChromatic { get; set; }
        /// <summary>UI language (a code from Localizer.Languages); null follows the device language.</summary>
        public string Language { get; set; }
        /// <summary>Light or dark colours, or those of the device.</summary>
        public ThemeMode Theme { get; set; } = ThemeMode.System;
        public string Error { get; set; }
        /// <summary>No settings were saved yet (first run).</summary>
        [JsonIgnore]
        public bool IsNew { get; private set; }

		public readonly NamingConvention[] NamingConventions = Enum.GetValues(typeof(NamingConvention))
																   .Cast<NamingConvention>()
																   .ToArray();
		public readonly Note[] Notes =
		{
			new Note(Tone.C),
			new Note(Tone.D),
			new Note(Tone.E),
			new Note(Tone.F),
			new Note(Tone.G),
			new Note(Tone.A),
			new Note(Tone.B)
		};

        /// <summary>The chosen instrument in its chosen tuning, with its capo on; frets of diagrams count from the capo.</summary>
        [JsonIgnore]
        public InstrumentInfo InstrumentInfo => InstrumentInfo.Get(Instrument, Tunings.GetValueOrDefault(Instrument), Capo);

        /// <summary>Fret the capo of the chosen instrument is on (0 = none).</summary>
        [JsonIgnore]
        public int Capo
        {
            get => Capos.GetValueOrDefault(Instrument);
            set
            {
                if (value <= 0)
                {
                    Capos.Remove(Instrument);
                }
                else
                {
                    Capos[Instrument] = Math.Min(value, InstrumentInfo.MaxCapo);
                }
            }
        }

        /// <summary>Tuning chosen for an instrument: a preset, a saved custom tuning, or an unnamed custom one.</summary>
        public InstrumentTuning TuningOf(Instrument instrument)
            => Describe(instrument, InstrumentInfo.Get(instrument, Tunings.GetValueOrDefault(instrument)).Pitches);

        /// <summary>Preset or saved custom tuning with these pitches, or an unnamed custom one.</summary>
        public InstrumentTuning Describe(Instrument instrument, int[] pitches)
            => InstrumentTuning.FindPreset(instrument, pitches)
               ?? CustomTunings.FirstOrDefault(i => i.Instrument == instrument && InstrumentTuning.SamePitches(i.Pitches, pitches))
               ?? new InstrumentTuning(null, instrument, pitches);

        /// <summary>Presets of the instrument followed by the player's own tunings.</summary>
        public IEnumerable<InstrumentTuning> TuningsFor(Instrument instrument)
            => InstrumentTuning.Presets(instrument).Concat(CustomTunings.Where(i => i.Instrument == instrument));

        public void SelectTuning(Instrument instrument, int[] pitches)
        {
            if (InstrumentInfo.Get(instrument, pitches).IsStandardTuning)
            {
                Tunings.Remove(instrument);
            }
            else
            {
                Tunings[instrument] = pitches.ToArray();
            }
        }

        public static SettingsModel Load()
        {
            SettingsModel result;

            try
            {
                result = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(FilePath));
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Cannot load settings\n{0}", e.Message));
                result = new SettingsModel { IsNew = true };
            }

            return result;
        }

		public void Save()
		{
			try
			{
				var data = JsonConvert.SerializeObject(this);

				CreateDirectoryForFilePath(FilePath);
				File.WriteAllText(FilePath, data);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Cannot load settings\n{0}", e.Message));
			}
		}

		private static void CreateDirectoryForFilePath(string path)
		{
			var dir = Path.GetDirectoryName(path);

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}
	}
}
