using System.Runtime.InteropServices;
using AVFoundation;
using Chords.App.Services;

namespace Chords.App;

/// <summary>
/// Records the microphone with a tap on <see cref="AVAudioEngine.InputNode"/>. While recording, the shared audio
/// session plays and records in measurement mode (no gain control, echo cancelling or filtering); afterwards it
/// goes back to playback only, as <see cref="PlatformSoundBank"/> set it up.
/// </summary>
public sealed class PlatformAudioInput : IAudioInput
{
    /// <summary>
    /// Samples per tap block (about 21 ms). The tap delivers a block only when it is full, so a larger one would make
    /// the needle wait for it; iOS may still hand out larger blocks than asked for.
    /// </summary>
    private const uint BlockSize = 1024;
    /// <summary>Hardware buffer asked for, so the tap can deliver its blocks this often.</summary>
    private const double IoBufferSeconds = 0.005;

    private readonly object _sync = new();
    private AVAudioEngine _engine;
    private float[] _block = new float[BlockSize];
    private bool _loggedBlock;

    public int Start(Action<float[], int> onSamples)
    {
        lock (_sync)
        {
            Stop();
            _loggedBlock = false;

            var session = AVAudioSession.SharedInstance();

            session.SetCategory(AVAudioSessionCategory.PlayAndRecord,
                AVAudioSessionCategoryOptions.DefaultToSpeaker | AVAudioSessionCategoryOptions.MixWithOthers
                | AVAudioSessionCategoryOptions.AllowBluetoothA2DP, out var error);
            Log("category", error);
            session.SetMode(AVAudioSession.ModeMeasurement, out error);
            Log("mode", error);
            session.SetPreferredIOBufferDuration(IoBufferSeconds, out error);
            Log("buffer", error);
            session.SetActive(true, out error);
            Log("activate", error);

            var engine = new AVAudioEngine();
            var input = engine.InputNode;
            var format = input.GetBusOutputFormat(0);

            if (format.SampleRate <= 0 || format.ChannelCount == 0)
            {
                RestorePlayback();
                return 0;
            }
            input.InstallTapOnBus(0, BlockSize, format, (buffer, _) =>
            {
                var count = (int)buffer.FrameLength;

                if (!_loggedBlock)
                {
                    _loggedBlock = true;
                    System.Diagnostics.Debug.WriteLine($"Tuner microphone: {count} samples per block at {format.SampleRate} Hz");
                }
                if (count == 0 || buffer.FloatChannelData == IntPtr.Zero)
                {
                    return;
                }
                if (_block.Length < count)
                {
                    _block = new float[count];
                }
                // First channel only: FloatChannelData points to one sample pointer per channel.
                Marshal.Copy(Marshal.ReadIntPtr(buffer.FloatChannelData), _block, 0, count);
                onSamples(_block, count);
            });
            engine.Prepare();
            if (!engine.StartAndReturnError(out error))
            {
                Log("start", error);
                input.RemoveTapOnBus(0);
                RestorePlayback();
                return 0;
            }
            _engine = engine;

            return (int)format.SampleRate;
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (_engine == null)
            {
                return;
            }
            _engine.InputNode.RemoveTapOnBus(0);
            _engine.Stop();
            _engine.Dispose();
            _engine = null;
            RestorePlayback();
        }
    }

    private static void RestorePlayback()
    {
        var session = AVAudioSession.SharedInstance();

        session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out var error);
        Log("category", error);
        session.SetMode(AVAudioSession.ModeDefault, out error);
        Log("mode", error);
    }

    private static void Log(string step, Foundation.NSError error)
    {
        if (error != null)
        {
            System.Diagnostics.Debug.WriteLine($"Audio session {step}: {error.LocalizedDescription}");
        }
    }
}
