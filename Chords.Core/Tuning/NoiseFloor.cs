using System;

namespace Chords.Core.Tuning
{
    /// <summary>
    /// Follows the loudness of the room's background noise, so a note counts as heard by how far it stands out of
    /// the noise rather than by a fixed level: the raw microphone (no gain control) records a quiet pluck in a quiet
    /// room far below what a fixed threshold safe for a noisy room would allow.
    /// The floor drops at once to any quieter frame and rises fast only on frames that are not a note, slowly on
    /// tonal ones: a held note does not lift it, but a steady hum is gated out after a while. It starts at the bottom:
    /// until it has risen to a noisy room's level, only clarity tells notes from noise, as it always did above the
    /// old fixed threshold.
    /// </summary>
    public class NoiseFloor
    {
        /// <summary>A note starts only this far above the floor.</summary>
        private const double StartDb = 10;
        /// <summary>A note that has started lasts until it decays to this far above the floor.</summary>
        private const double SustainDb = 4;
        /// <summary>How fast the floor rises on frames without a note.</summary>
        private const double NoiseRiseDbPerSecond = 10;
        /// <summary>How fast the floor rises on tonal frames (a held note, or a steady hum).</summary>
        private const double ToneRiseDbPerSecond = 1;

        private static readonly float StartRatio = (float)Math.Pow(10, StartDb / 20);
        private static readonly float SustainRatio = (float)Math.Pow(10, SustainDb / 20);

        /// <summary>Background noise level (RMS, 1 = full scale).</summary>
        public float Level { get; private set; } = PitchDetector.SilenceRms;

        /// <summary>Quietest frame that can start a note.</summary>
        public float StartThreshold => Level * StartRatio;

        /// <summary>Quietest frame that keeps a sounding note.</summary>
        public float SustainThreshold => Level * SustainRatio;

        public void Reset() => Level = PitchDetector.SilenceRms;

        /// <summary>Adds the loudness of one frame, <paramref name="frameRate"/> frames a second.</summary>
        public void Add(float rms, bool tonal, double frameRate)
        {
            if (rms <= Level)
            {
                Level = Math.Max(PitchDetector.SilenceRms, rms);
                return;
            }
            var rise = (tonal ? ToneRiseDbPerSecond : NoiseRiseDbPerSecond) / frameRate;

            Level = Math.Min(rms, Level * (float)Math.Pow(10, rise / 20));
        }
    }
}
