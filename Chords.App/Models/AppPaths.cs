namespace Chords.App.Models;

public static class AppPaths
{
    // On Android this is Context.FilesDir (/data/data/<package>/files), the same folder the
    // Xamarin version stored favorites.json and settings.json in, so updating users keep them.
    public static string File(string name) => Path.Combine(FileSystem.AppDataDirectory, name);
}
