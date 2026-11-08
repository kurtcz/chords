using Chords.Core.Models;
using Newtonsoft.Json;

namespace Chords.Models;

public static class Favorites
{
    public static readonly string FilePath = AppPaths.File("favorites.json");
    private static readonly object Sync = new();

    public static Dictionary<Chord, HashSet<GuitarChordLayout>> Chords { get; } = Load();

    private static Dictionary<Chord, HashSet<GuitarChordLayout>> Load()
    {
        var result = new Dictionary<Chord, HashSet<GuitarChordLayout>>();

        try
        {
            if (!File.Exists(FilePath))
            {
                return result;
            }
            var data = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<Chord, HashSet<GuitarChordLayout>>>>(File.ReadAllText(FilePath));

            foreach (var kv in data ?? [])
            {
                // Drop layouts whose diagram can't be rendered (corrupt or outdated data).
                kv.Value.RemoveWhere(layout =>
                {
                    try
                    {
                        return string.IsNullOrEmpty(new GuitarChordLayoutDecorator(layout).Schema.Value);
                    }
                    catch
                    {
                        return true;
                    }
                });
                result[kv.Key] = kv.Value;
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot load favorites\n{e.Message}");
        }

        return result;
    }

    public static bool Contains(Chord chord, GuitarChordLayout layout)
    {
        lock (Sync)
        {
            return Chords.TryGetValue(chord, out var set) && set.Contains(layout);
        }
    }

    /// <summary>Adds or removes the layout; returns whether it is a favorite afterwards.</summary>
    public static bool Toggle(Chord chord, GuitarChordLayout layout)
    {
        bool favorite;

        lock (Sync)
        {
            if (!Chords.TryGetValue(chord, out var set))
            {
                Chords[chord] = set = new HashSet<GuitarChordLayout>();
            }
            favorite = set.Add(layout) || !set.Remove(layout);
            Save();
        }

        return favorite;
    }

    private static void Save()
    {
        try
        {
            var data = JsonConvert.SerializeObject(Chords.Where(i => i.Value.Any()));

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, data);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot save favorites\n{e.Message}");
        }
    }
}
