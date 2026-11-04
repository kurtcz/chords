using Android.Media;
using Chords.App.Services;

namespace Chords.App;

/// <summary>Records the microphone with <see cref="AudioRecord"/> on a background thread.</summary>
public sealed class PlatformAudioInput : IAudioInput
{
    private const int BlockSize = 1024;

    private readonly object _sync = new();
    private AudioRecord _record;
    private Thread _thread;
    private volatile bool _running;

    public int Start(Action<float[], int> onSamples)
    {
        lock (_sync)
        {
            Stop();
            foreach (var rate in new[] { 48000, 44100 })
            {
                var record = Open(rate);

                if (record == null)
                {
                    continue;
                }
                record.StartRecording();
                if (record.RecordingState != RecordState.Recording)
                {
                    record.Release();
                    continue;
                }
                _record = record;
                _running = true;
                _thread = new Thread(() => Read(record, onSamples)) { IsBackground = true, Name = "Tuner microphone" };
                _thread.Start();

                return rate;
            }

            return 0;
        }
    }

    /// <summary>
    /// The unprocessed source skips the gain control and noise suppression that bend a tone's pitch;
    /// devices without it get voice recognition, the least processed of the others.
    /// </summary>
    private static AudioRecord Open(int rate)
    {
        var minBuffer = AudioRecord.GetMinBufferSize(rate, ChannelIn.Mono, Encoding.PcmFloat);

        if (minBuffer <= 0)
        {
            return null;
        }
        var audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
        var source = audioManager?.GetProperty(AudioManager.PropertySupportAudioSourceUnprocessed) == "true"
            ? AudioSource.Unprocessed
            : AudioSource.VoiceRecognition;
        var record = new AudioRecord(source, rate, ChannelIn.Mono, Encoding.PcmFloat, Math.Max(minBuffer, BlockSize * 4 * sizeof(float)));

        if (record.State != State.Initialized)
        {
            record.Release();
            return null;
        }

        return record;
    }

    private void Read(AudioRecord record, Action<float[], int> onSamples)
    {
        var block = new float[BlockSize];

        while (_running)
        {
            var count = record.Read(block, 0, block.Length, 0 /* READ_BLOCKING */);

            if (count < 0)
            {
                break;
            }
            if (count > 0 && _running)
            {
                onSamples(block, count);
            }
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            _running = false;
            if (_record == null)
            {
                return;
            }
            try
            {
                _record.Stop();
            }
            catch (Java.Lang.IllegalStateException)
            {
            }
            _thread?.Join(500);
            _record.Release();
            _record = null;
            _thread = null;
        }
    }
}
