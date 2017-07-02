using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Chords.Core.Models;
using System.Linq;

namespace Chords.Android
{
    public static class Favorites
    {
        //public static string FilePath = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, "favorites.json");
        public static string FilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "favorites.json");
        public static Dictionary<Chord, HashSet<GuitarChordLayout>> Chords { get; }

        static Favorites()
        {
            Chords = new Dictionary<Chord, HashSet<GuitarChordLayout>>();
			try
			{
                if (File.Exists(FilePath))
                {
                    Chords = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<Chord, HashSet<GuitarChordLayout>>>>(File.ReadAllText(FilePath))
                                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                }
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Cannot load favorites\n{0}", e.Message));
			}
		}

        public static void Init()
        {
            //calling this method will force the static constructor to be invoked
        }

        public static void Save()
        {
            var data = JsonConvert.SerializeObject(Chords.Where(i => i.Value.Any()));

            CreateDirectoryForFilePath(FilePath);
            File.WriteAllText(FilePath, data);
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
