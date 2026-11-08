using Chords.Localization;
using Chords.Services;

namespace Chords;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        builder.Services.AddSingleton<Localizer>();
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<ISoundBank, PlatformSoundBank>();
        builder.Services.AddSingleton<ChordPlayer>();
        builder.Services.AddSingleton<IAudioInput, PlatformAudioInput>();
        builder.Services.AddSingleton<TunerListener>();

        return builder.Build();
    }
}
