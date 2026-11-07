using Chords.Core.Models;

namespace Chords.App.Services;

public class ChordPlayer
{
    private readonly ISoundBank soundBank;
    private readonly AppState state;

    public ChordPlayer(ISoundBank soundBank, AppState state)
    {
        this.soundBank = soundBank;
        this.state = state;
        Prepare(state.Settings.Instrument);
    }

    /// <summary>Pause between strings of the first, slow strum and of the second, fast one (ms).</summary>
    private const int SlowStrumDelay = 350;
    private const int FastStrumDelay = 50;
    /// <summary>How long the last strum rings before another chord may start (ms).</summary>
    private const int RingDelay = 500;

    /// <summary>1 while a chord is playing; taps meanwhile are ignored so the sounds don't pile up.</summary>
    private int _playing;

    /// <summary>Samples loaded last; another instrument's replace them.</summary>
    private SampleSet _prepared;

    /// <summary>Note each string of the instrument sounds, so plucking the string again silences it first.</summary>
    private readonly Dictionary<int, int> _ringing = [];

    /// <summary>Samples of the instrument, loading them (and freeing another instrument's) the first time.</summary>
    private SampleSet Prepare(Instrument instrument)
    {
        var set = SampleSet.For(instrument);

        if (!ReferenceEquals(Interlocked.Exchange(ref _prepared, set), set))
        {
            lock (_ringing)
            {
                _ringing.Clear();
            }
            soundBank.Prepare(set.Names);
        }
        return set;
    }

    /// <summary>
    /// Plucks one string of a chord diagram (a finger strumming across it); a muted string (-1) stays silent.
    /// A string still ringing from before stops, as a real one does when plucked again.
    /// </summary>
    public void PlayString(InstrumentInfo instrument, int[] positions, int stringIndex, float volume = 1f)
    {
        if (positions[stringIndex] >= 0)
        {
            Pluck(Prepare(instrument.Instrument), stringIndex, instrument.Pitches[stringIndex] + instrument.Semitones(stringIndex, positions[stringIndex]), volume);
        }
    }

    private void Pluck(SampleSet set, int stringIndex, int pitch, float volume)
    {
        var (sample, rate) = set.For(pitch);

        lock (_ringing)
        {
            if (_ringing.TryGetValue(stringIndex, out var previous))
            {
                soundBank.Stop(previous);
            }
            _ringing[stringIndex] = soundBank.Play(sample, rate, volume);
        }
    }

    /// <summary>Strums a chord diagram twice (slow, then fast). -1 = muted string.</summary>
    public void PlayLayout(InstrumentInfo instrument, int[] positions, float volume = 1f) => PlayExclusive(async () =>
    {
        var set = Prepare(instrument.Instrument);

        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                if (positions[i] < 0)
                {
                    continue;
                }
                Pluck(set, i, instrument.Pitches[i] + instrument.Semitones(i, positions[i]), volume);
                await Task.Delay(pass == 0 ? SlowStrumDelay : FastStrumDelay);
            }
            await Task.Delay(pass == 0 ? 200 : RingDelay);
        }
    });

    /// <summary>
    /// Strums a chord diagram once, low string to high, e.g. on a beat of a progression played to the metronome
    /// (Pages/Progressions.razor). <paramref name="damp"/> first silences the chord before, whose notes would clash
    /// with this one's (C major's C and E against G major's B and D). It does not wait for another playback.
    /// </summary>
    /// <param name="spread">Pause between two strings (ms), shorter for a quick strum at a fast tempo.</param>
    public void Strum(InstrumentInfo instrument, int[] positions, bool damp, float volume = 1f, int spread = FastStrumDelay)
    {
        var set = Prepare(instrument.Instrument);

        Task.Run(async () =>
        {
            if (damp)
            {
                soundBank.StopAll();
            }
            for (var i = 0; i < positions.Length; i++)
            {
                if (positions[i] < 0)
                {
                    continue;
                }
                Pluck(set, i, instrument.Pitches[i] + instrument.Semitones(i, positions[i]), volume);
                if (spread > 0)
                {
                    await Task.Delay(spread);
                }
            }
        });
    }

    /// <summary>Silences every note still ringing.</summary>
    public void Damp() => soundBank.StopAll();

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
    /// Plays a single note (MIDI pitch) on an instrument (by default the chosen one), e.g. an open string
    /// as the tuner's reference, tuned to the given A4 reference pitch (Hz).
    /// </summary>
    public void PlayPitch(int pitch, float volume = 1f, double a4 = SampleSet.StandardA4, Instrument? instrument = null)
    {
        var (sample, rate) = Prepare(instrument ?? state.Settings.Instrument).For(pitch, a4);

        soundBank.Play(sample, rate, volume);
    }

    /// <summary>Plays the notes of a chord (English name, e.g. "F#m") as a quick arpeggio.</summary>
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
    /// Plays the notes of a chord on the chosen instrument as a quick rising arpeggio from its root
    /// (C3..B3, or the lowest octave the instrument's samples reach, e.g. C4..B4 on the ukulele).
    /// </summary>
    public void PlayChord(Chord chord, float volume = 1f) => PlayExclusive(async () =>
    {
        var set = Prepare(state.Settings.Instrument);
        var c = new Note(Tone.C);
        var bottom = ArpeggioBase;
        var previous = -1;

        while (bottom < set.Lowest)
        {
            bottom += 12;
        }
        foreach (var note in chord.Notes)
        {
            var pitch = bottom + c.ChromaticDistance(note);

            while (pitch <= previous)
            {
                pitch += 12;
            }
            previous = pitch;

            var (sample, rate) = set.For(pitch);

            soundBank.Play(sample, rate, volume);
            await Task.Delay(FastStrumDelay);
        }
        await Task.Delay(RingDelay);
    });
}
