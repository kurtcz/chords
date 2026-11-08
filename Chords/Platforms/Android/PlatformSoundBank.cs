using Android.Media;
using Chords.Services;

namespace Chords;

public sealed class PlatformSoundBank : Java.Lang.Object, ISoundBank, SoundPool.IOnLoadCompleteListener
{
    private const int MaxStreams = 12;

    private readonly object _sync = new();
    private readonly SoundPool _soundPool;
    /// <summary>SoundPool ids of the samples asked for, loaded or still loading.</summary>
    private readonly Dictionary<string, int> _soundIds = [];
    private readonly HashSet<int> _loaded = [];
    /// <summary>Notes played before their sample had loaded, started once it has.</summary>
    private readonly Dictionary<int, List<(float Rate, float Volume, int Id)>> _pending = [];
    /// <summary>Stream of each note id still able to sound (the last <see cref="MaxStreams"/>).</summary>
    private readonly List<(int Id, int Stream)> _streams = [];
    private int _lastId;

    public PlatformSoundBank()
    {
        _soundPool = new SoundPool.Builder()
            .SetMaxStreams(MaxStreams)
            .SetAudioAttributes(new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetContentType(AudioContentType.Music)
                .Build())
            .Build();
        _soundPool.SetOnLoadCompleteListener(this);
    }

    public void Prepare(IReadOnlyList<string> samples)
    {
        lock (_sync)
        {
            foreach (var (name, soundId) in _soundIds.Where(i => !samples.Contains(i.Key)).ToList())
            {
                _soundPool.Unload(soundId);
                _soundIds.Remove(name);
                _loaded.Remove(soundId);
                _pending.Remove(soundId);
            }
            foreach (var name in samples)
            {
                SoundId(name);
            }
        }
    }

    /// <summary>SoundPool id of a sample, starting to load it the first time (0 when the app lacks it). Call within the lock.</summary>
    private int SoundId(string name)
    {
        if (_soundIds.TryGetValue(name, out var soundId))
        {
            return soundId;
        }
        var context = Android.App.Application.Context;
        // Resource names are lower case ("E6.ogg" becomes raw/e6).
        var resource = context.Resources.GetIdentifier(name.ToLowerInvariant(), "raw", context.PackageName);

        soundId = resource == 0 ? 0 : _soundPool.Load(context, resource, 1);
        _soundIds[name] = soundId;

        return soundId;
    }

    public int Play(string sample, float rate, float volume = 1f)
    {
        lock (_sync)
        {
            var soundId = SoundId(sample);

            if (soundId == 0)
            {
                return 0;
            }
            var id = ++_lastId;

            if (_loaded.Contains(soundId))
            {
                Start(soundId, rate, volume, id);
            }
            else
            {
                if (!_pending.TryGetValue(soundId, out var notes))
                {
                    _pending[soundId] = notes = [];
                }
                notes.Add((rate, volume, id));
            }
            return id;
        }
    }

    public void OnLoadComplete(SoundPool soundPool, int sampleId, int status)
    {
        lock (_sync)
        {
            if (status != 0 || !_soundIds.ContainsValue(sampleId))
            {
                return;
            }
            _loaded.Add(sampleId);
            if (_pending.Remove(sampleId, out var notes))
            {
                foreach (var (rate, volume, id) in notes)
                {
                    Start(sampleId, rate, volume, id);
                }
            }
        }
    }

    /// <summary>Call within the lock.</summary>
    private void Start(int soundId, float rate, float volume, int id)
    {
        var stream = _soundPool.Play(soundId, volume, volume, 0, 0, rate);

        if (stream != 0)
        {
            _streams.Add((id, stream));
            // Only the last MaxStreams can still sound; older ones are long finished.
            if (_streams.Count > MaxStreams)
            {
                _streams.RemoveAt(0);
            }
        }
    }

    public void Stop(int id)
    {
        lock (_sync)
        {
            var index = _streams.FindIndex(s => s.Id == id);

            if (index >= 0)
            {
                _soundPool.Stop(_streams[index].Stream);
                _streams.RemoveAt(index);
            }
            foreach (var notes in _pending.Values)
            {
                notes.RemoveAll(n => n.Id == id);
            }
        }
    }

    public void StopAll()
    {
        lock (_sync)
        {
            foreach (var (_, stream) in _streams)
            {
                _soundPool.Stop(stream);
            }
            _streams.Clear();
            foreach (var notes in _pending.Values)
            {
                notes.Clear();
            }
        }
    }
}
