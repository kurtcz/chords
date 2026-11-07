using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;
using Xunit;

namespace Chords.Core.Tests;

public class FretboardTests
{
    [Fact]
    public void CMajorScaleOnTheLowEString()
    {
        var notes = new Scale(new Note(Tone.C), ScaleType.Major).Notes;
        var found = Fretboard.Find(InstrumentInfo.Guitar, notes, 0, 12).Where(i => i.String == 0).ToArray();

        Assert.Equal(new[] { 0, 1, 3, 5, 7, 8, 10, 12 }, found.Select(i => i.Fret));
        Assert.Equal(new[] { 2, 3, 4, 5, 6, 0, 1, 2 }, found.Select(i => i.Index));
        Assert.Equal(new[] { 40, 41, 43, 45, 47, 48, 50, 52 }, found.Select(i => i.Pitch));
    }

    [Fact]
    public void EnharmonicSpellingsMatch()
    {
        var found = Fretboard.Find(InstrumentInfo.Guitar, new[] { new Note(Tone.D, Accidental.Flat) }, 0, 12);

        Assert.Contains(found, i => i.String == 1 && i.Fret == 4);
    }

    [Fact]
    public void CapoHidesTheFretsBelowIt()
    {
        var notes = new Scale(new Note(Tone.D), ScaleType.Major).Notes;
        var found = Fretboard.Find(InstrumentInfo.Guitar, notes, 2, 12);

        Assert.All(found, i => Assert.True(i.Fret >= 2));
        //the A string rings B from the capo
        Assert.Contains(found, i => i.String == 1 && i.Fret == 2 && i.Pitch == 47);
    }

    [Fact]
    public void BanjoShortStringStartsAtItsNut()
    {
        var found = Fretboard.Find(InstrumentInfo.Banjo, new[] { new Note(Tone.G) }, 0, 12);

        Assert.Contains(found, i => i.String == 0 && i.Fret == 5);
        Assert.DoesNotContain(found, i => i.String == 0 && i.Fret < 5);
        Assert.Equal(5, Fretboard.OpenFret(InstrumentInfo.Banjo, 0, 2));
        Assert.Equal(7, Fretboard.OpenFret(InstrumentInfo.Banjo, 0, 7));
    }

    [Fact]
    public void PositionsStartWhereTheLowStringHasANote()
    {
        var notes = new Scale(new Note(Tone.A), ScaleType.Minor).Notes;

        Assert.Equal(new[] { 0, 1, 3, 5, 7, 8 }, Fretboard.Positions(InstrumentInfo.Guitar, notes, 0, 12));
        //the open position reaches the 4th fret, the others span four frets
        Assert.Equal(4, Fretboard.PositionEnd(InstrumentInfo.Guitar, 0, 0));
        Assert.Equal(8, Fretboard.PositionEnd(InstrumentInfo.Guitar, 5, 0));
    }

    [Fact]
    public void PositionsStartFromTheCapo()
    {
        var notes = new Scale(new Note(Tone.D), ScaleType.Major).Notes;
        var positions = Fretboard.Positions(InstrumentInfo.Guitar, notes, 2, 12);

        Assert.Equal(2, positions[0]);
        Assert.All(positions, i => Assert.True(i >= 2));
        Assert.Equal(6, Fretboard.PositionEnd(InstrumentInfo.Guitar, 2, 2));
    }

    [Fact]
    public void BanjoPositionsFollowTheLowFullString()
    {
        //D string (the 4th) is the lowest that runs from the nut
        var positions = Fretboard.Positions(InstrumentInfo.Banjo, new[] { new Note(Tone.D) }, 0, 12);

        Assert.Equal(new[] { 0 }, positions);
    }

    [Theory]
    [InlineData(Interval.P1, "1")]
    [InlineData(Interval.m3, "b3")]
    [InlineData(Interval.M3, "3")]
    [InlineData(Interval.A4, "#4")]
    [InlineData(Interval.d5, "b5")]
    [InlineData(Interval.m7, "b7")]
    [InlineData(Interval.d7, "bb7")]
    [InlineData(Interval.M9, "9")]
    [InlineData(Interval.A11, "#11")]
    [InlineData(Interval.m13, "b13")]
    public void DegreeNames(Interval interval, string name)
    {
        Assert.Equal(name, Fretboard.DegreeName(interval));
    }

    [Fact]
    public void MinorPentatonicDegrees()
    {
        Assert.Equal("1 b3 4 5 b7", string.Join(" ", ScaleType.MinorPentatonic.ToIntervals().Select(Fretboard.DegreeName)));
    }
}
