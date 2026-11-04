using System;
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
        /// <summary>Draw chord diagrams with horizontal strings (nut on the left), like looking at the neck.</summary>
        public bool Horizontal { get; set; } = false;
        /// <summary>Mirror horizontal chord diagrams so the nut is on the right.</summary>
        public bool LeftHanded { get; set; } = false;
        /// <summary>UI language (a code from Localizer.Languages); null follows the device language.</summary>
        public string Language { get; set; }
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
