using System.Linq;
using Chords.Core.Models;
using Newtonsoft.Json;
using Xunit;

namespace Chords.Core.Tests;

public class TuningTests
{
    private static Chord D => Chord.TryParse("D", NamingConvention.English, out var chord) ? chord : null!;

    [Fact]
    public void StandardPitchesGiveTheStandardInstrument()
    {
        Assert.Same(InstrumentInfo.Guitar, InstrumentInfo.Get(Instrument.Guitar, new[] { 40, 45, 50, 55, 59, 64 }));
        Assert.Same(InstrumentInfo.Guitar, InstrumentInfo.Get(Instrument.Guitar, null));
        Assert.Same(InstrumentInfo.Guitar, InstrumentInfo.Get(Instrument.Guitar, new[] { 40, 45 }));
    }

    [Fact]
    public void AlternateTuningIsCachedAndKeepsTheRules()
    {
        var dropD = InstrumentTuning.FindPreset(Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 })!;
        var info = dropD.Info;

        Assert.Same(info, InstrumentInfo.Get(Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 }));
        Assert.False(info.IsStandardTuning);
        Assert.Equal(Instrument.Guitar, info.Instrument);
        Assert.Equal(new Note(Tone.D), info.Strings[0]);
        Assert.Equal(-2, dropD.Offset(0));
    }

    [Fact]
    public void DropDPlaysTheLowStringOpenInD()
    {
        var info = InstrumentInfo.Get(Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 });
        var layouts = GuitarChordLayout.Generate(D, info, true, false, false, 5).ToArray();

        Assert.Contains(layouts, i => i.Positions.SequenceEqual(new[] { 0, 0, 0, 2, 3, 2 }));
        Assert.All(layouts, i => Assert.Same(info, i.InstrumentInfo));
    }

    [Fact]
    public void OpenGStrumsGOpen()
    {
        var info = InstrumentInfo.Get(Instrument.Guitar, new[] { 38, 43, 50, 55, 59, 62 });
        Chord.TryParse("G", NamingConvention.English, out var g);

        Assert.Contains(GuitarChordLayout.Generate(g, info, true, false, false, 5), i => i.Positions.All(p => p == 0));
    }

    [Fact]
    public void LayoutRemembersItsTuning()
    {
        var info = InstrumentInfo.Get(Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 });
        var layout = new GuitarChordLayout(D, new[] { 0, 0, 0, 2, 3, 2 }, null, info);
        var copy = JsonConvert.DeserializeObject<GuitarChordLayout>(JsonConvert.SerializeObject(layout))!;

        Assert.Equal(layout, copy);
        Assert.Same(info, copy.InstrumentInfo);
        Assert.NotEqual(layout, new GuitarChordLayout(D, new[] { 0, 0, 0, 2, 3, 2 }, null, InstrumentInfo.Guitar));
    }

    [Fact]
    public void FavoritesSavedBeforeTuningsAreStandard()
    {
        var standard = new GuitarChordLayout(D, new[] { -1, -1, 0, 2, 3, 2 });
        var json = JsonConvert.SerializeObject(standard);

        Assert.DoesNotContain("Tuning", json);
        Assert.Same(InstrumentInfo.Guitar, JsonConvert.DeserializeObject<GuitarChordLayout>(json)!.InstrumentInfo);
    }

    [Fact]
    public void HalfStepDownIsSpelledInFlats()
    {
        var eb = InstrumentTuning.Presets(Instrument.Guitar).Single(i => i.NameKey == "Tuning_HalfStepDown");

        Assert.Equal("Eb Ab Db Gb Bb Eb", string.Join(" ", Enumerable.Range(0, 6).Select(s => eb.NoteOf(s).ToString())));
        Assert.True(new InstrumentTuning("x", Instrument.Ukulele, new[] { 66, 59, 63, 68 }).Flats);
        Assert.True(new InstrumentTuning("x", Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 63 }).Flats);
        Assert.False(new InstrumentTuning("x", Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 65 }).Flats);
    }

    [Fact]
    public void EveryPresetFitsItsInstrument()
    {
        foreach (var instrument in System.Enum.GetValues<Instrument>())
        {
            var presets = InstrumentTuning.Presets(instrument);

            Assert.True(presets[0].IsStandard);
            Assert.All(presets, i => Assert.Equal(InstrumentInfo.Get(instrument).StringCount, i.Pitches.Length));
            Assert.Equal(presets.Count, presets.Select(i => string.Join(",", i.Pitches)).Distinct().Count());
        }
    }
}
