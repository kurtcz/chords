using System;
using System.Linq;
using Chords.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace Chords.Core.Tests;

public class VoiceLeadingTests
{
    private readonly ITestOutputHelper _output;

    public VoiceLeadingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>Fingerings ordered as the app shows them: by difficulty level, then up the neck.</summary>
    private static GuitarChordLayout[] Layouts(Chord chord, InstrumentInfo info = null) =>
        GuitarChordLayout.Generate(chord, info ?? InstrumentInfo.Guitar, true, false, true, 10)
                         .OrderBy(i => i.DifficultyLevel)
                         .ThenBy(i => i.Fret)
                         .ThenBy(i => i.Complete ? 0 : 1)
                         .ThenBy(i => Note.Normalize(i.Notes[0]) == Note.Normalize(chord.Root) ? 0 : 1)
                         .ThenBy(i => i.ToString())
                         .ToArray();

    private static GuitarChordLayout[][] Options(Progression progression, Note tonic, ScaleType scale, bool sevenths, InstrumentInfo info = null) =>
        progression.In(new Key(tonic, scale), sevenths).Select(i => Layouts(i.Chord, info)).ToArray();

    private static Progression Find(ProgressionStyle style, string numerals) =>
        Progression.Common.Single(i => i.Style == style && string.Join(" ", i.Numerals) == numerals);

    /// <summary>The widest jump of the hand between neighbouring chords, the last moving back to the first.</summary>
    private static double WidestJump(GuitarChordLayout[] chosen) =>
        chosen.Select((l, i) => VoiceLeading.Distance(l, chosen[(i + 1) % chosen.Length])).Max();

    private static GuitarChordLayout[] Pick(GuitarChordLayout[][] options, int[] indices) =>
        options.Select((o, i) => o[indices[i]]).ToArray();

    [Fact]
    public void OpenChordsInCStayOpen()
    {
        var options = Options(Find(ProgressionStyle.Pop, "I V vi IV"), new Note(Tone.C), ScaleType.Major, false);
        var chosen = Pick(options, VoiceLeading.Choose(options));

        //C, G, Am stay open; F needs the first fret anyway
        Assert.All(chosen, l => Assert.True(l.Positions.Where(p => p > 0).Max() <= 3, l.ToString()));
        Assert.All(chosen, l => Assert.True(l.Complete, l.ToString()));
    }

    [Theory]
    [InlineData(ProgressionStyle.Pop, "I V vi IV", Tone.B, Accidental.Flat, false)]
    [InlineData(ProgressionStyle.Pop, "I vi IV V", Tone.E, Accidental.Flat, false)]
    [InlineData(ProgressionStyle.Jazz, "ii7 V7 Imaj7", Tone.B, Accidental.Flat, true)]
    [InlineData(ProgressionStyle.Jazz, "Imaj7 vi7 ii7 V7", Tone.F, Accidental.Sharp, true)]
    [InlineData(ProgressionStyle.Blues, "I7 I7 I7 I7 IV7 IV7 I7 I7 V7 IV7 I7 V7", Tone.A, Accidental.Flat, true)]
    public void HandStaysInPlace(ProgressionStyle style, string numerals, Tone tone, Accidental accidental, bool sevenths)
    {
        var options = Options(Find(style, numerals), new Note(tone, accidental), ScaleType.Major, sevenths);
        var easiest = options.Select(o => o[0]).ToArray();
        var smooth = Pick(options, VoiceLeading.Choose(options));

        _output.WriteLine($"easiest: {string.Join(" | ", easiest.Select(i => i.ToString()))}  widest jump {WidestJump(easiest)}");
        _output.WriteLine($"smooth:  {string.Join(" | ", smooth.Select(i => i.ToString()))}  widest jump {WidestJump(smooth)}");

        Assert.True(WidestJump(smooth) <= 3, $"widest jump {WidestJump(smooth)}");
        Assert.True(WidestJump(smooth) <= WidestJump(easiest));
        //the shapes stay playable
        Assert.All(smooth, l => Assert.NotEqual(DifficultyLevel.Hard, l.DifficultyLevel));
    }

    [Fact]
    public void RepeatedChordKeepsItsFingering()
    {
        var options = Options(Find(ProgressionStyle.Blues, "I7 I7 I7 I7 IV7 IV7 I7 I7 V7 IV7 I7 V7"), new Note(Tone.E), ScaleType.Major, true);
        var chosen = Pick(options, VoiceLeading.Choose(options));

        _output.WriteLine(string.Join(" | ", chosen.Select(i => i.ToString())));
        Assert.Single(new[] { 0, 1, 2, 3, 6, 7, 10 }.Select(i => chosen[i].ToString()).Distinct());
    }

    [Fact]
    public void LockedChordIsKeptAndOthersFollowIt()
    {
        var options = Options(Find(ProgressionStyle.Pop, "I V vi IV"), new Note(Tone.G), ScaleType.Major, false);
        //G as the barre at the 3rd fret
        var barre = Array.FindIndex(options[0], i => i.ToString() == "3 5 5 4 3 3");
        Assert.True(barre >= 0);

        var indices = VoiceLeading.Choose(options, new int?[] { barre, null, null, null });
        var chosen = Pick(options, indices);

        Assert.Equal(barre, indices[0]);
        _output.WriteLine(string.Join(" | ", chosen.Select(i => i.ToString())));
        //the others move up to the barre instead of staying open at the nut
        Assert.All(chosen.Skip(1), l => Assert.True(VoiceLeading.Distance(chosen[0], l) <= 3, l.ToString()));
    }

    [Fact]
    public void PreferredFingeringCountsAsEasier()
    {
        var options = new[] { Layouts(new Chord(new Note(Tone.A), ChordType.Major)) };
        var first = VoiceLeading.Choose(options)[0];
        var rival = Enumerable.Range(0, options[0].Length)
                              .Where(i => i != first)
                              .OrderBy(i => VoiceLeading.ShapeCost(options[0][i]))
                              .First();
        bool Preferred(GuitarChordLayout l) => l == options[0][rival];
        var gap = VoiceLeading.ShapeCost(options[0][rival]) - VoiceLeading.ShapeCost(options[0][first]);

        Assert.Equal(VoiceLeading.ShapeCost(options[0][rival]) - 2, VoiceLeading.ShapeCost(options[0][rival], Preferred));
        Assert.Equal(gap < 2 ? rival : first, VoiceLeading.Choose(options, preferred: Preferred)[0]);
    }

    [Fact]
    public void ChordsWithoutFingeringsAreSkipped()
    {
        var c = Layouts(new Chord(new Note(Tone.C), ChordType.Major));
        var options = new[] { c, Array.Empty<GuitarChordLayout>(), c };

        var indices = VoiceLeading.Choose(options);

        Assert.Equal(0, indices[1]);
        Assert.Equal(indices[0], indices[2]);
    }

    [Theory]
    [InlineData(Instrument.Ukulele)]
    [InlineData(Instrument.Mandolin)]
    [InlineData(Instrument.Banjo)]
    public void OtherInstruments(Instrument instrument)
    {
        var info = InstrumentInfo.Get(instrument);
        var options = Options(Find(ProgressionStyle.Pop, "I V vi IV"), new Note(Tone.E, Accidental.Flat), ScaleType.Major, false, info);
        var smooth = Pick(options, VoiceLeading.Choose(options));

        _output.WriteLine(string.Join(" | ", smooth.Select(i => i.ToString())));
        Assert.True(WidestJump(smooth) <= 3);
    }
}
