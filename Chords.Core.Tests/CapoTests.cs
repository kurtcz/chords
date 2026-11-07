using System.Linq;
using Chords.Core.Models;
using Newtonsoft.Json;
using Xunit;

namespace Chords.Core.Tests;

public class CapoTests
{
    private static Chord Parse(string name) => Chord.TryParse(name, NamingConvention.English, out var chord) ? chord : null!;

    [Fact]
    public void CapoRaisesTheOpenStringsAndKeepsTheTuning()
    {
        var info = InstrumentInfo.Get(Instrument.Guitar, null, 2);

        Assert.Equal(2, info.Capo);
        Assert.Equal(new[] { 42, 47, 52, 57, 61, 66 }, info.Pitches);
        Assert.Equal(new Note(Tone.F, Accidental.Sharp), info.Strings[0]);
        Assert.True(info.IsStandardTuning);
        Assert.Same(InstrumentInfo.Guitar, info.Uncapoed);
        Assert.Same(info, InstrumentInfo.Guitar.WithCapo(2));
        Assert.Same(InstrumentInfo.Guitar, info.WithCapo(0));
    }

    [Fact]
    public void CapoKeepsAnAlternateTuning()
    {
        var dropD = new[] { 38, 45, 50, 55, 59, 64 };
        var info = InstrumentInfo.Get(Instrument.Guitar, dropD, 3);

        Assert.False(info.IsStandardTuning);
        Assert.Equal(dropD, info.Uncapoed.Pitches);
        Assert.Equal(dropD.Select(i => i + 3), info.Pitches);
    }

    [Fact]
    public void CapoIsClamped()
    {
        Assert.Same(InstrumentInfo.Guitar, InstrumentInfo.Get(Instrument.Guitar, null, -1));
        Assert.Equal(InstrumentInfo.MaxCapo, InstrumentInfo.Get(Instrument.Guitar, null, 20).Capo);
    }

    [Fact]
    public void DWithTheCapoOnTwoIsPlayedAsACShape()
    {
        var info = InstrumentInfo.Guitar.WithCapo(2);
        var layouts = GuitarChordLayout.Generate(Parse("D"), info, true, false, false, 5).ToArray();

        Assert.Contains(layouts, i => i.Positions.SequenceEqual(new[] { -1, 3, 2, 0, 1, 0 }));
        Assert.All(layouts, i => Assert.Equal(2, i.Capo));
    }

    [Fact]
    public void LayoutRemembersItsCapo()
    {
        var info = InstrumentInfo.Get(Instrument.Guitar, new[] { 38, 45, 50, 55, 59, 64 }, 2);
        var layout = new GuitarChordLayout(Parse("D"), new[] { -1, 3, 2, 0, 1, 0 }, null, info);
        var copy = JsonConvert.DeserializeObject<GuitarChordLayout>(JsonConvert.SerializeObject(layout))!;

        Assert.Equal(layout, copy);
        Assert.Same(info, copy.InstrumentInfo);
        Assert.DoesNotContain("Capo", JsonConvert.SerializeObject(new GuitarChordLayout(Parse("D"), new[] { -1, -1, 0, 2, 3, 2 })));
    }

    [Theory]
    [InlineData("D", -2, "C")]
    [InlineData("F", -3, "D")]
    [InlineData("C", -1, "B")]
    [InlineData("C", -2, "Bb")]
    [InlineData("F", -1, "E")]
    [InlineData("G", -1, "F#")]
    [InlineData("Ebm", -1, "Dm")]
    [InlineData("B", 2, "C#")]
    [InlineData("E", -1, "Eb")]
    public void ShapeOfAChordUnderACapo(string sounding, int halftones, string shape)
    {
        Assert.Equal(Parse(shape), Parse(sounding).Transpose(halftones));
    }
}
