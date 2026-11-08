using Chords.Core.Models;

namespace Chords.Services;

/// <summary>
/// Recorded notes an instrument is played from; any other note is the nearest one played faster or slower.
/// </summary>
public sealed class SampleSet
{
    /// <summary>
    /// Plays every instrument from its own samples rendered from a SoundFont (tools/render_samples.py);
    /// false goes back to the six original guitar recordings for all instruments.
    /// </summary>
    public static readonly bool UseInstrumentSamples = true;

    /// <summary>The six original recordings of the open guitar strings, low E to high E.</summary>
    public static readonly SampleSet Original = new(["E6", "A5", "D4", "G3", "B2", "E1"], InstrumentInfo.Guitar.Pitches, nearestBelow: true);

    private static readonly Dictionary<Instrument, SampleSet> Rendered = new()
    {
        // Ranges and step must match tools/render_samples.py.
        [Instrument.Guitar] = Range("guitar", 38, 89),
        [Instrument.Ukulele] = Range("ukulele", 59, 89),
        [Instrument.Mandolin] = Range("mandolin", 55, 94),
        [Instrument.BassGuitar] = Range("bass", 26, 65),
        [Instrument.Banjo] = Range("banjo", 48, 90),
    };

    /// <summary>Samples an instrument is played from.</summary>
    public static SampleSet For(Instrument instrument)
        => UseInstrumentSamples ? Rendered[instrument] : Original;

    /// <summary>Sound file names (without extension).</summary>
    public IReadOnlyList<string> Names { get; }
    /// <summary>MIDI pitch of each sample, ascending.</summary>
    public IReadOnlyList<int> Pitches { get; }
    /// <summary>Lowest recorded pitch.</summary>
    public int Lowest => Pitches[0];

    /// <summary>
    /// Shifts notes only up from the sample below, as the original guitar recordings always were
    /// (each string from its own recording); rendered sets shift from the nearest sample.
    /// </summary>
    private readonly bool _nearestBelow;

    private SampleSet(string[] names, int[] pitches, bool nearestBelow = false)
    {
        Names = names;
        Pitches = pitches;
        _nearestBelow = nearestBelow;
    }

    /// <summary>A note every 3 semitones from <paramref name="low"/>, so no note is shifted by more than one semitone.</summary>
    private static SampleSet Range(string instrument, int low, int high)
    {
        var pitches = Enumerable.Range(0, (high - low) / 3 + 1).Select(i => low + i * 3).ToArray();

        return new SampleSet(pitches.Select(p => $"{instrument}_{p}").ToArray(), pitches);
    }

    /// <summary>Reference pitch of A4 (Hz) the samples are tuned to.</summary>
    public const double StandardA4 = 440;

    /// <summary>
    /// The sample a note is played from and the rate that shifts it to the note.
    /// Android's SoundPool only plays at 0.5–2×, so notes beyond that sound an octave closer.
    /// </summary>
    public (string Sample, float Rate) For(int pitch, double a4 = StandardA4)
    {
        var index = _nearestBelow
            ? Math.Max(0, LastIndex(p => p <= pitch))
            : Enumerable.Range(0, Pitches.Count).MinBy(i => Math.Abs(Pitches[i] - pitch));
        // The samples are tuned to A4 = 440 Hz; another reference pitch scales every note alike.
        var rate = (float)(Math.Pow(2, (pitch - Pitches[index]) / 12.0) * a4 / StandardA4);

        while (rate > 2f)
        {
            rate /= 2;
        }
        while (rate < 0.5f)
        {
            rate *= 2;
        }

        return (Names[index], rate);
    }

    private int LastIndex(Func<int, bool> match)
    {
        for (var i = Pitches.Count - 1; i >= 0; i--)
        {
            if (match(Pitches[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
