using Chords.Core.Models;
using Chords.Core.Tuning;

namespace Chords.App.Services;

public enum TunerStatus
{
    Stopped,
    Listening,
    /// <summary>The user did not allow the microphone.</summary>
    Denied,
    /// <summary>The microphone could not be opened.</summary>
    Failed
}

/// <summary>
/// Feeds the microphone (low-passed while a string is locked, see <see cref="TunerMath.FilterCutoff"/>) to a
/// <see cref="PitchDetector"/> about 20 times a second and reports the steadied <see cref="TunerReading"/>s (null = silence) on a background thread.
/// </summary>
public sealed class TunerListener(IAudioInput input)
{
    /// <summary>Analyses per second.</summary>
    private const int Rate = 20;

    private readonly object _sync = new();
    private PitchTracker _tracker;
    private PitchDetector _detector;
    private LowPassFilter _filter;
    /// <summary>The filtered piece of the current block.</summary>
    private float[] _filtered = [];
    private float[] _buffer;
    private int _sampleRate;
    private int _hop;
    private int _sinceAnalysis;
    private TunerReading _last;

    public TunerStatus Status { get; private set; }

    /// <summary>Raised after each analysis whose reading differs from the previous one.</summary>
    public event Action<TunerReading> ReadingChanged;

    public async Task<TunerStatus> StartAsync(InstrumentInfo instrument, double a4, int lockedString)
    {
        Stop();

        var permission = await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            return status == PermissionStatus.Granted ? status : await Permissions.RequestAsync<Permissions.Microphone>();
        });

        if (permission != PermissionStatus.Granted)
        {
            return Status = TunerStatus.Denied;
        }
        lock (_sync)
        {
            _tracker = new PitchTracker(instrument) { A4 = a4, LockedString = lockedString };
            _sampleRate = 0;
        }

        var sampleRate = input.Start(OnSamples);

        if (sampleRate <= 0)
        {
            return Status = TunerStatus.Failed;
        }
        lock (_sync)
        {
            _sampleRate = sampleRate;
            CreateDetector();
        }

        return Status = TunerStatus.Listening;
    }

    public void Stop()
    {
        input.Stop();
        lock (_sync)
        {
            _last = null;
            _detector = null;
        }
        if (Status == TunerStatus.Listening)
        {
            Status = TunerStatus.Stopped;
        }
    }

    /// <summary>Changes what is tuned while listening.</summary>
    public void Configure(InstrumentInfo instrument, double a4, int lockedString)
    {
        lock (_sync)
        {
            if (_tracker == null)
            {
                return;
            }
            var rangeChanged = _tracker.Instrument != instrument || _tracker.A4 != a4;
            var lockChanged = _tracker.LockedString != lockedString;

            _tracker.Instrument = instrument;
            _tracker.A4 = a4;
            _tracker.LockedString = lockedString;
            if (rangeChanged)
            {
                _tracker.Reset();
                CreateDetector();
            }
            else if (lockChanged)
            {
                CreateFilter();
            }
        }
    }

    private void CreateDetector()
    {
        if (_sampleRate <= 0)
        {
            return;
        }
        var (min, max) = TunerMath.Range(_tracker.Instrument, _tracker.A4);

        _detector = new PitchDetector(_sampleRate, min, max);
        _buffer = new float[_detector.FrameSize];
        _hop = _sampleRate / Rate;
        _sinceAnalysis = 0;
        CreateFilter();
    }

    /// <summary>The cutoff follows the locked string, so a new filter whenever it changes; none in auto mode.</summary>
    private void CreateFilter()
    {
        if (_sampleRate <= 0)
        {
            return;
        }
        var cutoff = TunerMath.FilterCutoff(_tracker.Instrument, _tracker.LockedString, _tracker.A4);

        _filter = cutoff.HasValue ? new LowPassFilter(_sampleRate, cutoff.Value) : null;
    }

    /// <summary>Appends a block in pieces of at most one hop, analysing after each full hop.</summary>
    private void OnSamples(float[] block, int count)
    {
        var offset = 0;

        while (offset < count)
        {
            TunerReading reading;
            bool changed;

            lock (_sync)
            {
                if (_detector == null)
                {
                    return;
                }
                var n = Math.Min(count - offset, _hop - _sinceAnalysis);

                var source = block;
                var start = offset;

                if (_filter != null)
                {
                    if (_filtered.Length < n)
                    {
                        _filtered = new float[n];
                    }
                    // Every sample goes through the filter, even those dropped below: it keeps state between them.
                    _filter.Process(block, offset, n, _filtered);
                    source = _filtered;
                    start = 0;
                }
                if (n >= _buffer.Length)
                {
                    // A hop longer than the frame (high instruments): keep its newest samples.
                    Array.Copy(source, start + n - _buffer.Length, _buffer, 0, _buffer.Length);
                }
                else
                {
                    Array.Copy(_buffer, n, _buffer, 0, _buffer.Length - n);
                    Array.Copy(source, start, _buffer, _buffer.Length - n, n);
                }
                offset += n;
                _sinceAnalysis += n;
                if (_sinceAnalysis < _hop)
                {
                    continue;
                }
                _sinceAnalysis = 0;
                reading = _tracker.Add(_detector.Detect(_buffer));
                changed = !Same(reading, _last);
                _last = reading;
            }
            if (changed)
            {
                ReadingChanged?.Invoke(reading);
            }
        }
    }

    private static bool Same(TunerReading a, TunerReading b)
        => a == null || b == null ? a == b : a.Pitch == b.Pitch && a.Target == b.Target && a.String == b.String;
}
