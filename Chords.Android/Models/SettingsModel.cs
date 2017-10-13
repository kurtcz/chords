﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Chords.Core.Models;
using Newtonsoft.Json;

namespace Chords.Android.Models
{
    public class SettingsModel
    {
		public static string FilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "settings.json");
		public NamingConvention conv { get; set; } = NamingConvention.English;
        public int maxFret { get; set; } = 10;
        public bool ShowBasicChordTypes { get; set; } = true;
        public bool AllowBarre { get; set; } = true;
        public bool AllowSpecial { get; set; } = false;
        public bool ShowTips { get; set; } = true;
        public string Error { get; set; }

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

		public void Populate(NameValueCollection parameters)
        {
            NamingConvention conv;
            int maxFret;
            bool basicChordTypes;
            bool special;

            if(Enum.TryParse(parameters["conv"], out conv))
            {
                this.conv = conv;
            }
			if (int.TryParse(parameters["maxfret"], out maxFret))
			{
                this.maxFret = maxFret;
			}
            if (bool.TryParse(parameters["basictypes"], out basicChordTypes))
			{
				this.ShowBasicChordTypes = basicChordTypes;
			}
            if (bool.TryParse(parameters["special"], out special))
			{
                this.AllowSpecial = special;
			}
			if (bool.TryParse(parameters["barre"], out special))
			{
				this.AllowBarre = special;
			}
			if (bool.TryParse(parameters["tips"], out special))
			{
                this.ShowTips = special;
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
                result = new SettingsModel();
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
