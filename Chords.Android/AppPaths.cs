using System.IO;

namespace Chords.Android
{
    public static class AppPaths
    {
        // Xamarin.Android mapped Environment.SpecialFolder.Personal to Context.FilesDir
        // (/data/data/<package>/files). Newer .NET maps it differently, so use FilesDir
        // explicitly to keep reading the favorites/settings saved by older app versions.
        public static string DataDir => global::Android.App.Application.Context.FilesDir.AbsolutePath;

        public static string File(string name) => Path.Combine(DataDir, name);
    }
}
