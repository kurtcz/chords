using Android.Media;
using Chords.App.Services;

namespace Chords.App;

public sealed class PlatformSoundBank : ISoundBank
{
    private readonly SoundPool _soundPool;
    private readonly int[] _soundIds;

    public PlatformSoundBank()
    {
        var context = Android.App.Application.Context;

        _soundPool = new SoundPool.Builder()
            .SetMaxStreams(12)
            .SetAudioAttributes(new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetContentType(AudioContentType.Music)
                .Build())
            .Build();
        _soundIds =
        [
            _soundPool.Load(context, Resource.Raw.E6, 1),
            _soundPool.Load(context, Resource.Raw.A5, 1),
            _soundPool.Load(context, Resource.Raw.D4, 1),
            _soundPool.Load(context, Resource.Raw.G3, 1),
            _soundPool.Load(context, Resource.Raw.B2, 1),
            _soundPool.Load(context, Resource.Raw.E1, 1),
        ];
    }

    public void PlayString(int stringIndex, float rate, float volume = 1f)
        => _soundPool.Play(_soundIds[stringIndex], volume, volume, 0, 0, rate);
}
