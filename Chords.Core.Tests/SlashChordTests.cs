using System.Linq;
using Chords.Core.Models;
using Xunit;

namespace Chords.Core.Tests;

public class SlashChordTests
{
    private static Chord Parse(string symbol)
    {
        Assert.True(Chord.TryParse(symbol, NamingConvention.English, out var chord), symbol);
        return chord;
    }

    private static Note N(string note)
    {
        Assert.True(Note.TryParse(note, NamingConvention.English, out var result), note);
        return result;
    }

    /// <summary>Lowest sounding note of a fingering (by pitch, not by string – the ukulele is re-entrant).</summary>
    private static Note Lowest(GuitarChordLayout layout)
    {
        var info = layout.InstrumentInfo;
        var s = Enumerable.Range(0, info.StringCount)
                          .Where(i => layout.Positions[i] >= 0)
                          .OrderBy(i => info.Pitches[i] + info.Semitones(i, layout.Positions[i]))
                          .First();

        return Note.Normalize(info.NoteAt(s, layout.Positions[s]));
    }

    [Fact]
    public void ParsesSlashChord()
    {
        var chord = Parse("D/F#");

        Assert.Equal(N("D"), chord.Root);
        Assert.Equal(ChordType.Major, chord.ChordType);
        Assert.Equal(N("F#"), chord.Bass);
        Assert.True(chord.BassInChord);
        Assert.Equal(new[] { N("F#"), N("A"), N("D") }, chord.NotesFromBass);
    }

    [Fact]
    public void ParsesBassOutsideChord()
    {
        var chord = Parse("D/C");

        Assert.False(chord.BassInChord);
        Assert.Equal(N("C"), chord.NotesFromBass[0]);
        Assert.Equal(4, chord.NotesFromBass.Length);
    }

    [Fact]
    public void SixNineIsNotSlashChord()
    {
        var chord = Parse("C6/9");

        Assert.Null(chord.Bass);
        Assert.Equal(new Chord(N("C"), chord.ChordType), chord);
    }

    [Fact]
    public void RootAsBassIsRootPosition()
    {
        Assert.Null(Parse("C/C").Bass);
        Assert.Equal(Parse("C"), Parse("C/C"));
        Assert.NotEqual(Parse("C"), Parse("C/E"));
    }

    [Fact]
    public void TransposesBass()
    {
        var chord = Parse("C/E").Transpose(2);

        Assert.Equal(N("D"), chord.Root);
        Assert.Equal(N("F#"), chord.Bass);
    }

    [Theory]
    [InlineData("D/F#", Instrument.Guitar)]
    [InlineData("C/G", Instrument.Guitar)]
    [InlineData("C/E", Instrument.Guitar)]
    [InlineData("D/C", Instrument.Guitar)]
    [InlineData("Am/G", Instrument.Guitar)]
    [InlineData("C/E", Instrument.Ukulele)]
    public void GeneratedFingeringsSoundBassLowest(string symbol, Instrument instrument)
    {
        var chord = Parse(symbol);
        var layouts = GuitarChordLayout.Generate(chord, InstrumentInfo.Get(instrument), true, false, true, 10).ToArray();

        Assert.NotEmpty(layouts);
        Assert.All(layouts, i => Assert.Equal(Note.Normalize(chord.Bass), Lowest(i)));
        Assert.All(layouts, i => Assert.False(i.OtherBass));
    }

    [Fact]
    public void BassOutsideChordIsPlayedOnce()
    {
        var chord = Parse("D/C");
        var layouts = GuitarChordLayout.Generate(chord, InstrumentInfo.Guitar, true, false, true, 10).ToArray();

        Assert.NotEmpty(layouts);
        Assert.All(layouts, layout => Assert.Equal(1, layout.Notes.Count(i => Note.Normalize(i) == N("C"))));
    }

    [Fact]
    public void RootPositionFingeringsMarkInversions()
    {
        var layouts = GuitarChordLayout.Generate(Parse("C"), InstrumentInfo.Guitar, true, false, true, 10).ToArray();

        Assert.Contains(layouts, i => !i.OtherBass);
        Assert.Contains(layouts, i => i.OtherBass);
        Assert.All(layouts.Where(i => i.OtherBass), i => Assert.NotEqual(N("C"), Note.Normalize(i.Bass)));
    }

    [Theory]
    [InlineData("V/7", "G", "B")]
    [InlineData("vi/5", "A", "G")]
    [InlineData("I/3", "C", "E")]
    [InlineData("IV/b7", "F", "Bb")]
    public void ResolvesSlashNumeral(string numeral, string root, string bass)
    {
        var chord = Progression.Resolve(new Key(N("C"), ScaleType.Major), numeral, false, ScaleType.Major);

        Assert.Equal(N(root), chord.Chord.Root);
        Assert.Equal(N(bass), chord.Chord.Bass);
        Assert.Equal(numeral, chord.Numeral);
    }

    [Fact]
    public void MinorDescendingBass()
    {
        var progression = Progression.Common.Single(i => !i.ForMajor && string.Join(" ", i.Numerals) == "i i/7 VI V");
        var chords = progression.In(new Key(N("A"), ScaleType.Minor), false).Select(i => i.Chord).ToArray();

        Assert.Equal(new[] { N("A"), N("G"), N("F"), N("E") }, chords.Select(i => Note.Normalize(i.LowestNote)));
    }

    [Fact]
    public void SlashProgressionsArePlayableInEveryKey()
    {
        foreach (var progression in Progression.Common.Where(i => i.Numerals.Any(n => n.Contains('/'))))
        {
            foreach (var tonic in Note.AllNotesNormalized)
            {
                var key = new Key(tonic, progression.ForScale ?? (progression.ForMajor ? ScaleType.Major : ScaleType.Minor));

                foreach (var chord in progression.In(key, false))
                {
                    Assert.True(GuitarChordLayout.Generate(chord.Chord, InstrumentInfo.Guitar, true, false, true, 12).Any(),
                                $"{string.Join(" ", progression.Numerals)} in {tonic}: {chord.Numeral}");
                }
            }
        }
    }
}
