using AVFoundation;
using Chords.Services;
using Foundation;

namespace Chords;

/// <summary>
/// Mirrors Android's SoundPool: each voice is a player node behind a varispeed unit, which
/// (like SoundPool's rate) shifts pitch and speed together.
/// </summary>
public sealed class PlatformSoundBank : ISoundBank
{
    private const int VoiceCount = 12;
    /// <summary>Rendered samples are AAC (.m4a), the original guitar recordings WAV.</summary>
    private static readonly string[] Extensions = ["m4a", "wav"];

    private readonly object _sync = new();
    private readonly AVAudioEngine _engine = new();
    /// <summary>Every sample is mono at 44.1 kHz, read as 32-bit float.</summary>
    private readonly AVAudioFormat _format = new(44100, 1);
    private readonly Dictionary<string, AVAudioPcmBuffer> _buffers = [];
    private readonly (AVAudioPlayerNode Player, AVAudioUnitVarispeed Varispeed)[] _voices;
    /// <summary>Id of the note each voice plays (or last played).</summary>
    private readonly int[] _voiceIds = new int[VoiceCount];
    private int _nextVoice;
    private int _lastId;
    /// <summary>Samples asked for by the last <see cref="Prepare"/>; the others are freed.</summary>
    private HashSet<string> _wanted = [];

    public PlatformSoundBank()
    {
        AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out _);

        _voices = new (AVAudioPlayerNode, AVAudioUnitVarispeed)[VoiceCount];
        for (var i = 0; i < VoiceCount; i++)
        {
            var player = new AVAudioPlayerNode();
            var varispeed = new AVAudioUnitVarispeed();

            _engine.AttachNode(player);
            _engine.AttachNode(varispeed);
            _engine.Connect(player, varispeed, _format);
            _engine.Connect(varispeed, _engine.MainMixerNode, _format);
            _voices[i] = (player, varispeed);
        }
    }

    public void Prepare(IReadOnlyList<string> samples)
    {
        var wanted = samples.ToHashSet();

        lock (_sync)
        {
            _wanted = wanted;
            // A voice still playing a freed buffer keeps it until it ends.
            foreach (var name in _buffers.Keys.Where(n => !wanted.Contains(n)).ToList())
            {
                _buffers.Remove(name);
            }
        }
        Task.Run(() =>
        {
            foreach (var name in samples)
            {
                lock (_sync)
                {
                    if (!ReferenceEquals(_wanted, wanted))
                    {
                        return;
                    }
                }
                Buffer(name);
            }
        });
    }

    /// <summary>The decoded sample, loading it when needed (decoding runs outside the lock).</summary>
    private AVAudioPcmBuffer Buffer(string name)
    {
        lock (_sync)
        {
            if (_buffers.TryGetValue(name, out var loaded))
            {
                return loaded;
            }
        }
        var buffer = Load(name);

        lock (_sync)
        {
            if (_buffers.TryGetValue(name, out var loaded))
            {
                return loaded;
            }
            if (buffer != null && _wanted.Contains(name))
            {
                _buffers[name] = buffer;
            }
        }
        return buffer;
    }

    private static AVAudioPcmBuffer Load(string name)
    {
        var url = Extensions.Select(e => NSBundle.MainBundle.GetUrlForResource(name, e)).FirstOrDefault(u => u != null);

        if (url == null)
        {
            System.Diagnostics.Debug.WriteLine($"Missing sample {name}");
            return null;
        }
        using var file = new AVAudioFile(url, out var error);

        if (error != null)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot load {name}: {error.LocalizedDescription}");
            return null;
        }
        var buffer = new AVAudioPcmBuffer(file.ProcessingFormat, (uint)file.Length);

        file.ReadIntoBuffer(buffer, out error);

        return error == null ? buffer : null;
    }

    public int Play(string sample, float rate, float volume = 1f)
    {
        var buffer = Buffer(sample);

        if (buffer == null)
        {
            return 0;
        }
        lock (_sync)
        {
            if (!_engine.Running && !_engine.StartAndReturnError(out var error))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot start audio engine: {error?.LocalizedDescription}");
                return 0;
            }
            var voice = _nextVoice;
            var (player, varispeed) = _voices[voice];

            _nextVoice = (_nextVoice + 1) % VoiceCount;
            player.Stop();
            varispeed.Rate = rate;
            player.Volume = volume;
            player.ScheduleBuffer(buffer, () => { });
            player.Play();

            return _voiceIds[voice] = ++_lastId;
        }
    }

    public void Stop(int id)
    {
        if (id == 0)
        {
            return;
        }
        lock (_sync)
        {
            var voice = Array.IndexOf(_voiceIds, id);

            if (voice >= 0)
            {
                _voices[voice].Player.Stop();
            }
        }
    }

    public void StopAll()
    {
        lock (_sync)
        {
            foreach (var (player, _) in _voices)
            {
                player.Stop();
            }
        }
    }
}
