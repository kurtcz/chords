using Chords.Core.Models;

namespace Chords.App.Services;

public class ChordPlayer(ISoundBank soundBank)
{
    /// <summary>MIDI pitches of the recorded open guitar strings (sound bank indexes 0..5).</summary>
    private static readonly int[] SamplePitches = InstrumentInfo.Guitar.Pitches;

    /// <summary>Pause between strings of the first, slow strum and of the second, fast one (ms).</summary>
    private const int SlowStrumDelay = 350;
    private const int FastStrumDelay = 50;
    /// <summary>How long the last strum rings before another chord may start (ms).</summary>
    private const int RingDelay = 500;

    /// <summary>1 while a chord is playing; taps meanwhile are ignored so the sounds don't pile up.</summary>
    private int _playing;

    /// <summary>Strums a chord diagram twice (slow, then fast). -1 = muted string.</summary>
    public void PlayLayout(InstrumentInfo instrument, int[] positions, float volume = 1f) => PlayExclusive(async () =>
    {
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                if (positions[i] < 0)
                {
                    continue;
                }
                var (sample, rate) = SampleFor(instrument.Pitches[i], instrument.Semitones(i, positions[i]));

                soundBank.PlayString(sample, rate, volume);
                await Task.Delay(pass == 0 ? SlowStrumDelay : FastStrumDelay);
            }
            await Task.Delay(pass == 0 ? 200 : RingDelay);
        }
    });

    /// <summary>Pause between chords of a progression (ms).</summary>
    private const int ChordDelay = 1100;

    /// <summary>Set to stop a progression after the chord being played.</summary>
    private volatile bool _stop;

    /// <summary>
    /// Strums the chord diagrams of a progression one after another (each once).
    /// Called while a progression is playing, it stops it instead.
    /// </summary>
    public void PlaySequence(InstrumentInfo instrument, IReadOnlyList<int[]> layouts, float volume = 1f)
    {
        if (Volatile.Read(ref _playing) != 0)
        {
            _stop = true;
            return;
        }
        _stop = false;
        PlayExclusive(async () =>
        {
            var first = true;

            foreach (var positions in layouts)
            {
                if (_stop)
                {
                    break;
                }
                // Damp the previous chord: left ringing, its notes clash with the next one
                // (C major's C and E against G major's B and D).
                if (!first)
                {
                    soundBank.StopAll();
                }
                first = false;

                var strings = 0;

                for (var i = 0; i < positions.Length; i++)
                {
                    if (positions[i] < 0)
                    {
                        continue;
                    }
                    var (sample, rate) = SampleFor(instrument.Pitches[i], instrument.Semitones(i, positions[i]));

                    soundBank.PlayString(sample, rate, volume);
                    strings++;
                    await Task.Delay(FastStrumDelay);
                }
                await Task.Delay(Math.Max(0, ChordDelay - strings * FastStrumDelay));
            }
        });
    }

    /// <summary>Runs a playback unless another one is still in progress.</summary>
    private void PlayExclusive(Func<Task> play)
    {
        if (Interlocked.CompareExchange(ref _playing, 1, 0) != 0)
        {
            return;
        }
        Task.Run(async () =>
        {
            try
            {
                await play();
            }
            finally
            {
                Volatile.Write(ref _playing, 0);
            }
        });
    }

    /// <summary>
    /// Picks the highest recorded string not above the open string (on the guitar its own recording)
    /// and the rate that shifts it to the played note.
    /// Android's SoundPool only plays at 0.5–2×, so notes beyond that sound an octave closer.
    /// </summary>
    private static (int Sample, float Rate) SampleFor(int openPitch, int fret)
    {
        var sample = Math.Max(0, Array.FindLastIndex(SamplePitches, p => p <= openPitch));
        var rate = (float)Math.Pow(2, (openPitch + fret - SamplePitches[sample]) / 12.0);

        while (rate > 2f)
        {
            rate /= 2;
        }
        while (rate < 0.5f)
        {
            rate *= 2;
        }

        return (sample, rate);
    }

    /// <summary>Plays a single note (MIDI pitch), e.g. an open string as the tuner's reference.</summary>
    public void PlayPitch(int pitch, float volume = 1f)
    {
        var (sample, rate) = SampleFor(pitch, 0);

        soundBank.PlayString(sample, rate, volume);
    }

    /// <summary>Plays the notes of a chord (English name, e.g. "F#m") as a quick arpeggio on the A string.</summary>
    public void PlayChord(string id, float volume = 1f)
    {
        if (Chord.TryParse(id, NamingConvention.English, out var chord))
        {
            PlayChord(chord, volume);
        }
    }

    /// <summary>MIDI pitch of C3, the lowest root of <see cref="PlayChord(Chord, float)"/>.</summary>
    private const int ArpeggioBase = 48;

    /// <summary>
    /// Plays the notes of a chord as a quick rising arpeggio from its root (C3..B3).
    /// Each note is shifted from the nearest recorded string below it, so the rate stays
    /// within SoundPool's 0.5–2× (shifting everything from the A string pushed e.g. the B and D
    /// of G major past 2×, where Android clamped them both to A).
    /// </summary>
    public void PlayChord(Chord chord, float volume = 1f) => PlayExclusive(async () =>
    {
        var c = new Note(Tone.C);
        var previous = -1;

        foreach (var note in chord.Notes)
        {
            var pitch = ArpeggioBase + c.ChromaticDistance(note);

            while (pitch <= previous)
            {
                pitch += 12;
            }
            previous = pitch;

            var (sample, rate) = SampleFor(pitch, 0);

            soundBank.PlayString(sample, rate, volume);
            await Task.Delay(FastStrumDelay);
        }
        await Task.Delay(RingDelay);
    });
}
