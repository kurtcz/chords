using Android.Media;
using Chords.App.Services;

namespace Chords.App;

public sealed class PlatformSoundBank : ISoundBank
{
    private const int MaxStreams = 12;

    private readonly SoundPool _soundPool;
    private readonly int[] _soundIds;
    /// <summary>Streams started since the last <see cref="StopAll"/>.</summary>
    private readonly List<int> _streams = [];

    public PlatformSoundBank()
    {
        var context = Android.App.Application.Context;

        _soundPool = new SoundPool.Builder()
            .SetMaxStreams(MaxStreams)
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
    {
        var stream = _soundPool.Play(_soundIds[stringIndex], volume, volume, 0, 0, rate);

        lock (_streams)
        {
            if (stream != 0)
            {
                _streams.Add(stream);
                // Only the last MaxStreams can still sound; older ids are long finished.
                if (_streams.Count > MaxStreams)
                {
                    _streams.RemoveAt(0);
                }
            }
        }
    }

    public void StopAll()
    {
        lock (_streams)
        {
            foreach (var stream in _streams)
            {
                _soundPool.Stop(stream);
            }
            _streams.Clear();
        }
    }
}
