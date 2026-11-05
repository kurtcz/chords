using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Models;

namespace Chords.Core.Tuning
{
    /// <summary>Equal-temperament conversions between frequencies, MIDI pitches and notes.</summary>
    public static class TunerMath
    {
        public const float DefaultA4 = 440f;
        /// <summary>MIDI pitch of A4.</summary>
        private const int A4Pitch = 69;

        /// <summary>Fractional MIDI pitch of a frequency (69.0 = A4, 69.5 = a quarter tone above).</summary>
        public static double PitchOf(double frequency, double a4 = DefaultA4)
            => A4Pitch + 12 * Math.Log(frequency / a4, 2);

        public static double FrequencyOf(double pitch, double a4 = DefaultA4)
            => a4 * Math.Pow(2, (pitch - A4Pitch) / 12);

        /// <summary>Note of a MIDI pitch, spelled with sharps.</summary>
        public static Note NoteOf(int pitch) => Note.AllNotesNormalized[Mod(pitch, 12)];

        /// <summary>Scientific octave number (C4 = middle C = MIDI 60).</summary>
        public static int OctaveOf(int pitch) => (int)Math.Floor(pitch / 12.0) - 1;

        /// <summary>Open string closest to a pitch.</summary>
        public static int NearestString(InstrumentInfo instrument, double pitch)
        {
            var best = 0;

            for (var s = 1; s < instrument.StringCount; s++)
            {
                if (Math.Abs(instrument.Pitches[s] - pitch) < Math.Abs(instrument.Pitches[best] - pitch))
                {
                    best = s;
                }
            }

            return best;
        }

        /// <summary>
        /// Frequencies the tuner listens to for an instrument: from a fourth below its lowest string
        /// (detuned strings) to an octave above its highest.
        /// </summary>
        public static (float Min, float Max) Range(InstrumentInfo instrument, double a4 = DefaultA4)
            => ((float)FrequencyOf(instrument.Pitches.Min() - 5, a4), (float)FrequencyOf(instrument.Pitches.Max() + 12, a4));

        /// <summary>Cutoff of the <see cref="LowPassFilter"/>, in harmonics of the locked string.</summary>
        private const double CutoffHarmonics = 6;

        /// <summary>
        /// Low-pass cutoff for the microphone with a locked string: keeps its fundamental and lowest harmonics and
        /// drops the upper partials, which run sharp. Null in auto mode, where the note is not known: a cutoff low
        /// enough for the low strings smooths the attack of the high ones into octave-low readings.
        /// </summary>
        public static float? FilterCutoff(InstrumentInfo instrument, int lockedString, double a4 = DefaultA4)
            => lockedString >= 0 && lockedString < instrument.StringCount
                ? (float)(CutoffHarmonics * FrequencyOf(instrument.Pitches[lockedString], a4))
                : (float?)null;

        private static int Mod(int a, int n) => ((a % n) + n) % n;
    }

    /// <summary>What the tuner shows: the note heard and how far it is from the target.</summary>
    public class TunerReading
    {
        public double Frequency { get; }
        /// <summary>Fractional MIDI pitch heard.</summary>
        public double Pitch { get; }
        /// <summary>Pitch the player tunes to: the nearest semitone, or the locked string.</summary>
        public int Target { get; }
        /// <summary>Distance from the target in cents (negative = too low, tune up).</summary>
        public double Cents { get; }
        /// <summary>String whose open pitch is the target, -1 when the note is no string's.</summary>
        public int String { get; }

        public TunerReading(double frequency, double pitch, int target, int stringIndex)
        {
            Frequency = frequency;
            Pitch = pitch;
            Target = target;
            Cents = (pitch - target) * 100;
            String = stringIndex;
        }
    }

    /// <summary>
    /// Turns noisy per-frame pitch estimates into a steady reading: ignores silence and unclear frames,
    /// takes the median of the last few estimates (an occasional octave jump never shows) and forgets
    /// the note after a short pause.
    /// </summary>
    public class PitchTracker
    {
        /// <summary>Estimates less periodic than this (noise, the attack of a pluck) are skipped.</summary>
        public const float MinClarity = 0.85f;
        /// <summary>Estimates the median is taken of.</summary>
        private const int History = 5;
        /// <summary>Frames without a pitch after which the note is gone.</summary>
        private const int MaxGap = 8;
        /// <summary>A string is shown as the target only when the note is within this many semitones of it.</summary>
        private const double StringCapture = 2.5;

        /// <summary>
        /// A note starts only when two frames in a row agree within this many semitones: the first frames of
        /// a pluck still hold the silence before it and the attack, and often read a wrong note.
        /// </summary>
        private const double ConfirmTolerance = 0.5;

        /// <summary>With a locked string, estimates this many semitones or more from it are ignored (octave errors).</summary>
        private const double OctaveGuard = 11.5;

        /// <summary>
        /// In chromatic mode the target moves to the next semitone only this far past it (semitones; 0.5 is halfway):
        /// a note right between two semitones would otherwise flip between them.
        /// </summary>
        private const double ChromaticHysteresis = 0.55;

        private readonly List<double> _pitches = new List<double>(History);
        /// <summary>First estimate of a possible new note, waiting for a second one to confirm it.</summary>
        private double? _pending;
        private int _gap;
        /// <summary>Semitone the chromatic reading targets, kept while the note stays near it.</summary>
        private int? _chromaticTarget;

        public InstrumentInfo Instrument { get; set; }
        public double A4 { get; set; } = TunerMath.DefaultA4;
        /// <summary>String to tune regardless of the note heard; -1 picks the nearest string or semitone.</summary>
        public int LockedString { get; set; } = -1;
        /// <summary>Targets the nearest semitone and ignores the strings (and <see cref="LockedString"/>).</summary>
        public bool Chromatic { get; set; }

        public PitchTracker(InstrumentInfo instrument)
        {
            Instrument = instrument;
        }

        public void Reset()
        {
            _pitches.Clear();
            _pending = null;
            _gap = 0;
            _chromaticTarget = null;
        }

        /// <summary>Adds an estimate; returns the current reading, null when nothing is sounding.</summary>
        public TunerReading Add(PitchEstimate estimate)
        {
            var pitch = estimate.HasPitch ? TunerMath.PitchOf(estimate.Frequency, A4) : 0;

            if (estimate.HasPitch && estimate.Clarity >= MinClarity && !IsOctaveOffLockedString(pitch))
            {
                // A different note than the ones heard: start over so the old note doesn't linger.
                if (_pitches.Count > 0 && Math.Abs(pitch - Median()) > 1 && _gap > 0)
                {
                    _pitches.Clear();
                }
                if (_pitches.Count > 0)
                {
                    if (_pitches.Count == History)
                    {
                        _pitches.RemoveAt(0);
                    }
                    _pitches.Add(pitch);
                }
                else if (_pending.HasValue && Math.Abs(pitch - _pending.Value) <= ConfirmTolerance)
                {
                    // Two frames agree: the note has started.
                    _pitches.Add(_pending.Value);
                    _pitches.Add(pitch);
                    _pending = null;
                }
                else
                {
                    _pending = pitch;
                }
                _gap = 0;
            }
            else if (++_gap > MaxGap)
            {
                _pitches.Clear();
                _pending = null;
                _chromaticTarget = null;
            }

            return Current;
        }

        public TunerReading Current
        {
            get
            {
                if (_pitches.Count == 0)
                {
                    return null;
                }
                var pitch = Median();
                var frequency = TunerMath.FrequencyOf(pitch, A4);

                if (Chromatic)
                {
                    return new TunerReading(frequency, pitch, ChromaticTarget(pitch), -1);
                }
                if (LockedString >= 0 && LockedString < Instrument.StringCount)
                {
                    return new TunerReading(frequency, pitch, Instrument.Pitches[LockedString], LockedString);
                }
                var nearest = TunerMath.NearestString(Instrument, pitch);

                if (Math.Abs(pitch - Instrument.Pitches[nearest]) <= StringCapture)
                {
                    return new TunerReading(frequency, pitch, Instrument.Pitches[nearest], nearest);
                }

                return new TunerReading(frequency, pitch, (int)Math.Round(pitch), -1);
            }
        }

        /// <summary>
        /// An octave or more from the locked string: an octave error, never the string itself (no string is tuned
        /// that far off). When the fundamental is weak (a phone speaker playing a low string) the attack can briefly
        /// read the 2nd or 3rd harmonic; a smoothed attack of a high string can read an octave low.
        /// </summary>
        private bool IsOctaveOffLockedString(double pitch)
            => LockedString >= 0 && LockedString < Instrument.StringCount
               && Math.Abs(pitch - Instrument.Pitches[LockedString]) >= OctaveGuard;

        private int ChromaticTarget(double pitch)
        {
            if (!_chromaticTarget.HasValue || Math.Abs(pitch - _chromaticTarget.Value) > ChromaticHysteresis)
            {
                _chromaticTarget = (int)Math.Round(pitch);
            }

            return _chromaticTarget.Value;
        }

        private double Median()
        {
            var sorted = _pitches.OrderBy(i => i).ToArray();
            var mid = sorted.Length / 2;

            return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
        }
    }
}
