using Chords.Core.Models;
using Chords.Core.Tuning;
using Xunit.Abstractions;

namespace Chords.Core.Tests;

public class PitchDetectorTests(ITestOutputHelper output)
{
    private static float[] Signal(int sampleRate, double seconds, Func<double, double> wave)
    {
        var samples = new float[(int)(sampleRate * seconds)];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)wave(i / (double)sampleRate);
        }

        return samples;
    }

    private static double Cents(double measured, double expected) => 1200 * Math.Log2(measured / expected);

    private static PitchDetector For(InstrumentInfo instrument, int sampleRate)
    {
        var (min, max) = TunerMath.Range(instrument);

        return new PitchDetector(sampleRate, min, max);
    }

    public static IEnumerable<object[]> OpenStrings()
    {
        foreach (var (name, instrument) in new[]
                 {
                     ("guitar", InstrumentInfo.Guitar), ("ukulele", InstrumentInfo.Ukulele),
                     ("mandolin", InstrumentInfo.Mandolin), ("bass", InstrumentInfo.BassGuitar),
                     ("banjo", InstrumentInfo.Banjo)
                 })
        {
            foreach (var pitch in instrument.Pitches.Distinct())
            {
                foreach (var rate in new[] { 44100, 48000 })
                {
                    yield return [name, pitch, rate];
                }
            }
        }
    }

    private static InstrumentInfo Instrument(string name) => name switch
    {
        "ukulele" => InstrumentInfo.Ukulele,
        "mandolin" => InstrumentInfo.Mandolin,
        "bass" => InstrumentInfo.BassGuitar,
        "banjo" => InstrumentInfo.Banjo,
        _ => InstrumentInfo.Guitar
    };

    [Theory]
    [MemberData(nameof(OpenStrings))]
    public void SineOfEveryOpenStringIsAccurateToACent(string instrument, int pitch, int sampleRate)
    {
        var detector = For(Instrument(instrument), sampleRate);
        // 7 cents flat, so the test also checks the direction.
        var frequency = TunerMath.FrequencyOf(pitch - 0.07);
        var samples = Signal(sampleRate, 0.2, t => 0.5 * Math.Sin(2 * Math.PI * frequency * t));

        var estimate = detector.Detect(samples);

        Assert.True(estimate.HasPitch);
        Assert.InRange(Cents(estimate.Frequency, frequency), -1, 1);
        Assert.True(estimate.Clarity > 0.95);
    }

    [Theory]
    [InlineData(40, 44100)] // E2, guitar
    [InlineData(28, 48000)] // E1, bass
    [InlineData(43, 44100)] // G1... G2 bass G string
    [InlineData(55, 48000)] // G3
    public void StrongSecondHarmonicDoesNotReadAnOctaveHigh(int pitch, int sampleRate)
    {
        var instrument = pitch < 40 ? InstrumentInfo.BassGuitar : InstrumentInfo.Guitar;
        var detector = For(instrument, sampleRate);
        var f = TunerMath.FrequencyOf(pitch);
        var samples = Signal(sampleRate, 0.2, t =>
            0.15 * Math.Sin(2 * Math.PI * f * t) +
            0.6 * Math.Sin(2 * Math.PI * 2 * f * t + 0.3) +
            0.3 * Math.Sin(2 * Math.PI * 3 * f * t + 1.1) +
            0.15 * Math.Sin(2 * Math.PI * 4 * f * t + 2.0));

        var estimate = detector.Detect(samples);

        Assert.True(estimate.HasPitch);
        Assert.InRange(Cents(estimate.Frequency, f), -2, 2);
    }

    /// <summary>Karplus–Strong plucked string: decaying, with harmonics, like a real pluck.</summary>
    [Theory]
    [InlineData(40)]
    [InlineData(45)]
    [InlineData(50)]
    [InlineData(55)]
    [InlineData(59)]
    [InlineData(64)]
    public void SynthesizedPluckReadsItsNote(int pitch)
    {
        const int sampleRate = 48000;
        var detector = For(InstrumentInfo.Guitar, sampleRate);
        var period = (int)Math.Round(sampleRate / TunerMath.FrequencyOf(pitch));
        // Averaging each sample with the following one shortens the loop by half a sample.
        var expected = sampleRate / (period - 0.5);
        var random = new Random(pitch);
        var line = Enumerable.Range(0, period).Select(_ => random.NextDouble() * 2 - 1).ToArray();
        var samples = new float[sampleRate];

        for (var i = 0; i < samples.Length; i++)
        {
            var j = i % period;
            var next = line[(j + 1) % period];

            samples[i] = (float)(line[j] * 0.5);
            line[j] = 0.996 * 0.5 * (line[j] + next);
        }

        var tracker = new PitchTracker(InstrumentInfo.Guitar);
        TunerReading reading = null;

        for (var end = detector.FrameSize + 4800; end <= samples.Length; end += 1024)
        {
            reading = tracker.Add(detector.Detect(samples, end)) ?? reading;
        }

        Assert.NotNull(reading);
        Assert.InRange(Cents(reading.Frequency, expected), -3, 3);
        Assert.Equal(Array.IndexOf(InstrumentInfo.Guitar.Pitches, pitch), reading.String);
    }

    [Fact]
    public void SilenceHasNoPitch()
    {
        var detector = For(InstrumentInfo.Guitar, 44100);

        Assert.False(detector.Detect(new float[detector.FrameSize]).HasPitch);
    }

    [Fact]
    public void NoiseIsNotAcceptedAsANote()
    {
        var detector = For(InstrumentInfo.Guitar, 44100);
        var random = new Random(1);
        var tracker = new PitchTracker(InstrumentInfo.Guitar);
        var samples = Signal(44100, 1, _ => random.NextDouble() - 0.5);

        for (var end = detector.FrameSize; end <= samples.Length; end += 1024)
        {
            Assert.Null(tracker.Add(detector.Detect(samples, end)));
        }
    }

    /// <summary>
    /// Filters like <c>TunerListener</c> and reads the steadied pitch every 1024 samples.
    /// </summary>
    private static List<TunerReading> Listen(float[] samples, int sampleRate, InstrumentInfo instrument, int lockedString,
        bool filtered = true)
    {
        var detector = For(instrument, sampleRate);
        var tracker = new PitchTracker(instrument) { LockedString = lockedString };
        tracker.FrameRate = sampleRate / 1024.0;
        var input = filtered ? TunerFilter.Apply(samples, sampleRate, TunerMath.FilterBand(instrument, lockedString)) : samples;
        var readings = new List<TunerReading>();

        for (var end = detector.FrameSize; end <= input.Length; end += 1024)
        {
            var reading = tracker.Add(detector.Detect(input, end));

            if (reading != null)
            {
                readings.Add(reading);
            }
        }

        return readings;
    }

    private static double MedianCents(IEnumerable<TunerReading> readings)
    {
        var cents = readings.Select(r => r.Cents).OrderBy(c => c).ToArray();

        return cents[cents.Length / 2];
    }

    /// <summary>
    /// The E2 reference tone played by the phone's speaker and recorded by its microphone. The speaker leaves the
    /// fundamental about 50 dB down, and the string's upper partials run sharp; unfiltered this read +14 cents.
    /// </summary>
    [Fact]
    public void ReferenceToneFromPhoneSpeakerReadsInTune()
    {
        var (samples, sampleRate) = Wav.Read(Path.Combine(AppContext.BaseDirectory, "Samples", "E2-phone-speaker.wav"));

        var unfiltered = Listen(samples, sampleRate, InstrumentInfo.Guitar, 0, filtered: false);
        var locked = Listen(samples, sampleRate, InstrumentInfo.Guitar, 0);

        output.WriteLine($"unfiltered {MedianCents(unfiltered):+0.0;-0.0}, filtered {MedianCents(locked):+0.0;-0.0} cents");
        Assert.True(MedianCents(unfiltered) > 10, "the recording no longer shows the sharp reading the filter fixes");
        Assert.NotEmpty(locked);
        Assert.InRange(MedianCents(locked), -3, 3);
        Assert.All(locked, r => Assert.InRange(r.Cents, -50, 50));
    }

    /// <summary>The filter leaves the period of a harmonic-rich tone alone on every string of every instrument.</summary>
    [Theory]
    [MemberData(nameof(OpenStrings))]
    public void FilteredOpenStringIsAccurate(string instrument, int pitch, int sampleRate)
    {
        var info = Instrument(instrument);
        var frequency = TunerMath.FrequencyOf(pitch - 0.07);
        // Sawtooth-like: 12 harmonics falling as 1/k.
        var samples = Signal(sampleRate, 0.5, t => Enumerable.Range(1, 12)
            .Sum(k => 0.3 / k * Math.Sin(2 * Math.PI * k * frequency * t + k)));

        var reading = Listen(samples, sampleRate, info, Array.IndexOf(info.Pitches, pitch)).Last();

        Assert.InRange(Cents(reading.Frequency, frequency), -1, 1);
    }

    /// <summary>The app's own recordings of the six guitar strings (E6 = low E … E1 = high E).</summary>
    [Theory]
    [InlineData("E6", 40)]
    [InlineData("A5", 45)]
    [InlineData("D4", 50)]
    [InlineData("G3", 55)]
    [InlineData("B2", 59)]
    [InlineData("E1", 64)]
    public void RecordedGuitarStringReadsItsNote(string file, int pitch)
    {
        var (samples, sampleRate) = Wav.Read(Path.Combine(AppContext.BaseDirectory, "Samples", file + ".wav"));

        // Locked to its string, through the filter.
        var locked = Listen(samples, sampleRate, InstrumentInfo.Guitar, Array.IndexOf(InstrumentInfo.Guitar.Pitches, pitch));

        Assert.All(locked, r => Assert.Equal(pitch, (int)Math.Round(r.Pitch)));
        Assert.InRange(MedianCents(locked), -5, 5);

        var readings = Listen(samples, sampleRate, InstrumentInfo.Guitar, -1);

        Assert.NotEmpty(readings);
        var wrong = readings.Count(r => Math.Round(r.Pitch) != pitch);
        var cents = readings.Select(r => r.Pitch * 100 - pitch * 100).OrderBy(c => c).ToArray();

        output.WriteLine($"{file}: {readings.Count} readings, median {cents[cents.Length / 2]:+0.0;-0.0} cents, " +
                         $"spread {cents[cents.Length / 10]:0.0}…{cents[cents.Length * 9 / 10]:0.0}, wrong note {wrong}");
        Assert.True(wrong <= readings.Count / 20, $"{wrong} of {readings.Count} readings on a wrong note");
        Assert.InRange(cents[cents.Length / 2], -25, 25);
        Assert.All(readings, r => Assert.Equal(Array.IndexOf(InstrumentInfo.Guitar.Pitches, pitch), r.String));
    }

    /// <summary>
    /// The recordings 30 dB quieter, in a quiet room's noise: a soft pluck far from the phone. A fixed gate at −50 dBFS
    /// heard almost nothing of them; against the measured noise floor they read nearly as long as the loud ones.
    /// </summary>
    [Theory]
    [InlineData("E6", 40)]
    [InlineData("D4", 50)]
    [InlineData("E1", 64)]
    public void QuietRecordedStringReadsItsNote(string file, int pitch)
    {
        var (samples, sampleRate) = Wav.Read(Path.Combine(AppContext.BaseDirectory, "Samples", file + ".wav"));
        var random = new Random(pitch);
        var loudRms = Math.Sqrt(samples.Average(x => (double)x * x));
        // 30 dB down, plus white noise at −75 dBFS.
        var quiet = samples.Select(x => (float)(x * 0.0316 + (random.NextDouble() - 0.5) * 0.000178 * Math.Sqrt(12))).ToArray();

        var loud = Listen(samples, sampleRate, InstrumentInfo.Guitar, -1);
        var readings = Listen(quiet, sampleRate, InstrumentInfo.Guitar, -1);

        output.WriteLine($"{file}: loud {20 * Math.Log10(loudRms):0} dBFS, {loud.Count} readings; quiet {readings.Count} readings, " +
                         $"median {MedianCents(readings):+0.0;-0.0} cents");
        Assert.True(readings.Count >= loud.Count * 2 / 3, $"{readings.Count} of {loud.Count} readings");
        var wrong = readings.Count(r => Math.Round(r.Pitch) != pitch);
        Assert.True(wrong <= readings.Count / 20, $"{wrong} of {readings.Count} readings on a wrong note");
        Assert.InRange(MedianCents(readings) - MedianCents(loud), -3, 3);
    }

    [Fact]
    public void QuietSineStandingOutOfTheNoiseIsHeard()
    {
        const int sampleRate = 48000;
        var detector = For(InstrumentInfo.Guitar, sampleRate);
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { FrameRate = sampleRate / 960.0 };
        var random = new Random(3);
        var f = TunerMath.FrequencyOf(45);
        // −63 dBFS A2 over −80 dBFS noise; the old fixed gate (−50 dBFS) never let it through.
        var samples = Signal(sampleRate, 1, t => 0.001 * Math.Sin(2 * Math.PI * f * t) + (random.NextDouble() - 0.5) * 0.0003);
        TunerReading reading = null;

        for (var end = detector.FrameSize; end <= samples.Length; end += 960)
        {
            reading = tracker.Add(detector.Detect(samples, end)) ?? reading;
        }

        Assert.NotNull(reading);
        Assert.InRange(reading.Cents, -1, 1);
    }

    [Fact]
    public void ToneBuriedInLoudNoiseIsNotAcceptedAsANote()
    {
        const int sampleRate = 48000;
        var detector = For(InstrumentInfo.Guitar, sampleRate);
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { FrameRate = sampleRate / 960.0 };
        var random = new Random(4);

        // A noisy room (−40 dBFS): the floor rises to it within seconds.
        var noise = Signal(sampleRate, 5, _ => (random.NextDouble() - 0.5) * 0.035);

        for (var end = detector.FrameSize; end <= noise.Length; end += 960)
        {
            Assert.Null(tracker.Add(detector.Detect(noise, end)));
        }
        Assert.InRange(20 * Math.Log10(tracker.Noise.Level), -43, -37);
    }
}

public class TunerTests(ITestOutputHelper output)
{
    [Fact]
    public void ConvertsBetweenFrequencyAndPitch()
    {
        Assert.Equal(69, TunerMath.PitchOf(440), 6);
        Assert.Equal(57, TunerMath.PitchOf(220), 6);
        Assert.Equal(82.407, TunerMath.FrequencyOf(40), 3);
        Assert.Equal(69, TunerMath.PitchOf(442, 442), 6);
        Assert.Equal(442, TunerMath.FrequencyOf(69, 442), 6);
    }

    [Theory]
    [InlineData(40, Tone.E, Accidental.Natural, 2)]
    [InlineData(60, Tone.C, Accidental.Natural, 4)]
    [InlineData(70, Tone.A, Accidental.Sharp, 4)]
    [InlineData(28, Tone.E, Accidental.Natural, 1)]
    [InlineData(23, Tone.B, Accidental.Natural, 0)]
    public void NamesPitches(int pitch, Tone tone, Accidental accidental, int octave)
    {
        Assert.Equal(new Note(tone, accidental), TunerMath.NoteOf(pitch));
        Assert.Equal(octave, TunerMath.OctaveOf(pitch));
    }

    [Fact]
    public void FindsNearestString()
    {
        Assert.Equal(0, TunerMath.NearestString(InstrumentInfo.Guitar, 38.5));
        Assert.Equal(1, TunerMath.NearestString(InstrumentInfo.Guitar, 44.2));
        Assert.Equal(5, TunerMath.NearestString(InstrumentInfo.Guitar, 70));
        // Re-entrant ukulele: the high G is the first string.
        Assert.Equal(0, TunerMath.NearestString(InstrumentInfo.Ukulele, 66.6));
        // Banjo drone G4 is string 0, the low D3 string 1.
        Assert.Equal(1, TunerMath.NearestString(InstrumentInfo.Banjo, 49.7));
    }

    private static PitchEstimate At(double pitch, float clarity = 0.98f)
        => new((float)TunerMath.FrequencyOf(pitch), clarity, 0.1f);

    /// <summary>Two agreeing frames: enough for a note to start.</summary>
    private static TunerReading Hear(PitchTracker tracker, double pitch)
    {
        tracker.Add(At(pitch));

        return tracker.Add(At(pitch));
    }

    [Fact]
    public void LockedStringIgnoresOctaveErrors()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { LockedString = 0 };

        // The 2nd and 3rd harmonic of the low E, and an octave below it: never the string itself.
        Assert.Null(Hear(tracker, 52.1));
        Assert.Null(Hear(tracker, 59));
        Assert.Null(Hear(tracker, 28));
        // A string tuned far off still reads.
        Assert.Equal(-500, Hear(tracker, 35).Cents, 3);
    }

    [Fact]
    public void NoteStartsOnlyWhenTwoFramesAgree()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        Assert.Null(tracker.Add(At(52)));
        Assert.Null(tracker.Add(At(58)));
        Assert.Null(tracker.Add(At(64)));
        Assert.Equal(64.05, tracker.Add(At(64.1)).Pitch, 3);
    }

    [Fact]
    public void ReadingTargetsTheNearestStringWithSignedCents()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        var reading = Hear(tracker, 44.8);

        Assert.Equal(1, reading.String);
        Assert.Equal(45, reading.Target);
        Assert.Equal(-20, reading.Cents, 3);
    }

    [Fact]
    public void LockedStringIsTheTargetEvenFarAway()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { LockedString = 0 };

        var reading = Hear(tracker, 43);

        Assert.Equal(0, reading.String);
        Assert.Equal(300, reading.Cents, 3);
    }

    [Fact]
    public void NoteFarFromAnyStringHasNoString()
    {
        var tracker = new PitchTracker(InstrumentInfo.Mandolin);

        var reading = Hear(tracker, 86.1);

        Assert.Equal(-1, reading.String);
        Assert.Equal(86, reading.Target);
    }

    [Fact]
    public void MedianIgnoresASingleOctaveJump()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        tracker.Add(At(40.1));
        tracker.Add(At(40.1));
        var reading = tracker.Add(At(52.1));

        Assert.Equal(40.1, reading.Pitch, 3);
    }

    [Fact]
    public void UnclearFramesAreSkippedAndSilenceForgetsTheNote()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        Hear(tracker, 40);
        Assert.NotNull(tracker.Add(At(50, clarity: 0.5f)));
        Assert.Equal(40, tracker.Current.Pitch, 3);
        // 0.4 s at 50 frames a second.
        for (var i = 0; i < 21; i++)
        {
            tracker.Add(default);
        }
        Assert.Null(tracker.Current);
    }

    [Fact]
    public void LessClearFramesKeepASoundingNoteButDoNotStartOne()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        // A decaying string: clear enough to keep the note, not to start one.
        tracker.Add(At(45.1, clarity: 0.75f));
        Assert.Null(tracker.Add(At(45.1, clarity: 0.75f)));
        Hear(tracker, 45);
        for (var i = 0; i < 40; i++)
        {
            tracker.Add(At(45.2, clarity: 0.75f));
        }
        Assert.Equal(45.2, tracker.Current.Pitch, 3);
        // An unclear frame of another note does not count.
        for (var i = 0; i < 40; i++)
        {
            tracker.Add(At(52, clarity: 0.75f));
        }
        Assert.Null(tracker.Current);
    }

    /// <summary>
    /// A steady note wavers by a few cents from frame to frame; at 50 readings a second the needle trembled. The
    /// shown pitch averages that out, yet still follows a peg turn within a few frames.
    /// </summary>
    [Fact]
    public void SteadyNoteDoesNotTrembleButAPegTurnShowsAtOnce()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { FrameRate = 50 };
        var random = new Random(5);
        var shown = new List<double>();

        // ±3 cents of wavering around A2 −20 cents.
        for (var i = 0; i < 100; i++)
        {
            var reading = tracker.Add(At(45 - 0.2 + (random.NextDouble() - 0.5) * 0.06));

            if (reading != null)
            {
                shown.Add(reading.Cents);
            }
        }
        var steady = shown.Skip(20).ToArray();
        output.WriteLine($"steady: {steady.Min():0.0}…{steady.Max():0.0} cents");
        Assert.InRange(steady.Max() - steady.Min(), 0, 2);

        // The peg turned to +10 cents.
        var turned = Enumerable.Range(0, 5).Select(_ => tracker.Add(At(45.1)).Cents).ToArray();
        output.WriteLine($"turned: {string.Join(" ", turned.Select(c => c.ToString("0.0")))}");
        Assert.InRange(turned[3], 7, 10.01);
    }

    [Fact]
    public void NewNoteAfterAPauseReplacesTheOldOne()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar);

        for (var i = 0; i < 5; i++)
        {
            tracker.Add(At(40));
        }
        tracker.Add(default);
        var reading = Hear(tracker, 45);

        Assert.Equal(45, reading.Pitch, 3);
    }

    [Fact]
    public void ChromaticTargetsTheNearestSemitoneNotAString()
    {
        // D2 on a guitar: two semitones below the low E, which auto mode would show as a flat E.
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { Chromatic = true, LockedString = 0 };

        var reading = Hear(tracker, 38.1);

        Assert.Equal(-1, reading.String);
        Assert.Equal(38, reading.Target);
        Assert.Equal(10, reading.Cents, 3);
    }

    [Fact]
    public void ChromaticTargetDoesNotFlipBetweenSemitones()
    {
        var tracker = new PitchTracker(InstrumentInfo.Guitar) { Chromatic = true };

        Assert.Equal(45, Hear(tracker, 45.45).Target);
        // Just past halfway the target stays; well past it, it moves.
        for (var i = 0; i < 30; i++)
        {
            tracker.Add(At(45.52));
        }
        Assert.Equal(45, tracker.Current.Target);
        Assert.Equal(52, tracker.Current.Cents, 0);
        for (var i = 0; i < 5; i++)
        {
            tracker.Add(At(45.6));
        }
        Assert.Equal(46, tracker.Current.Target);
    }

    [Fact]
    public void RangeCoversDetunedStrings()
    {
        var (min, max) = TunerMath.Range(InstrumentInfo.BassGuitar);

        Assert.InRange(min, 30, 31);
        Assert.InRange(max, 195, 197);
    }
}

/// <summary>Minimal reader of 16-bit PCM WAV files (first channel).</summary>
internal static class Wav
{
    public static (float[] Samples, int SampleRate) Read(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));

        reader.ReadBytes(12); // RIFF header
        int sampleRate = 0, channels = 1, bits = 16;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var id = new string(reader.ReadChars(4));
            var size = reader.ReadInt32();

            if (id == "fmt ")
            {
                reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadBytes(6);
                bits = reader.ReadInt16();
                reader.ReadBytes(size - 16);
            }
            else if (id == "data")
            {
                Assert.Equal(16, bits);
                var count = size / 2 / channels;
                var samples = new float[count];

                for (var i = 0; i < count; i++)
                {
                    samples[i] = reader.ReadInt16() / 32768f;
                    reader.ReadBytes(2 * (channels - 1));
                }

                return (samples, sampleRate);
            }
            else
            {
                reader.ReadBytes(size + (size & 1));
            }
        }

        throw new InvalidDataException("No data chunk.");
    }
}
