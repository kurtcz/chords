using AVFoundation;
using Chords.App.Services;
using Foundation;

namespace Chords.App;

/// <summary>
/// Mirrors Android's SoundPool: each voice is a player node behind a varispeed unit, which
/// (like SoundPool's rate) shifts pitch and speed together.
/// </summary>
public sealed class PlatformSoundBank : ISoundBank
{
    private const int VoiceCount = 12;
    private static readonly string[] Files = ["E6", "A5", "D4", "G3", "B2", "E1"];

    private readonly object _sync = new();
    private readonly AVAudioEngine _engine = new();
    private readonly AVAudioPcmBuffer[] _buffers;
    private readonly (AVAudioPlayerNode Player, AVAudioUnitVarispeed Varispeed)[] _voices;
    private int _nextVoice;

    public PlatformSoundBank()
    {
        AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out _);
        _buffers = Files.Select(LoadBuffer).ToArray();

        var format = _buffers[0].Format;

        _voices = new (AVAudioPlayerNode, AVAudioUnitVarispeed)[VoiceCount];
        for (var i = 0; i < VoiceCount; i++)
        {
            var player = new AVAudioPlayerNode();
            var varispeed = new AVAudioUnitVarispeed();

            _engine.AttachNode(player);
            _engine.AttachNode(varispeed);
            _engine.Connect(player, varispeed, format);
            _engine.Connect(varispeed, _engine.MainMixerNode, format);
            _voices[i] = (player, varispeed);
        }
    }

    private static AVAudioPcmBuffer LoadBuffer(string name)
    {
        var url = NSBundle.MainBundle.GetUrlForResource(name, "wav");
        var file = new AVAudioFile(url, out var error);

        if (error != null)
        {
            throw new InvalidOperationException($"Cannot load {name}.wav: {error.LocalizedDescription}");
        }
        var buffer = new AVAudioPcmBuffer(file.ProcessingFormat, (uint)file.Length);

        file.ReadIntoBuffer(buffer, out error);

        return buffer;
    }

    public void PlayString(int stringIndex, float rate, float volume = 1f)
    {
        lock (_sync)
        {
            if (!_engine.Running && !_engine.StartAndReturnError(out var error))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot start audio engine: {error?.LocalizedDescription}");
                return;
            }
            var (player, varispeed) = _voices[_nextVoice];

            _nextVoice = (_nextVoice + 1) % VoiceCount;
            player.Stop();
            varispeed.Rate = rate;
            player.Volume = volume;
            player.ScheduleBuffer(_buffers[stringIndex], () => { });
            player.Play();
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
