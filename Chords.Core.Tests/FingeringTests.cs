using System.Linq;
using Chords.Core.Models;
using Xunit;

namespace Chords.Core.Tests;

public class FingeringTests
{
    [Theory]
    [InlineData("x 3 2 0 1 0", "0 3 2 0 1 0")] // C
    [InlineData("3 2 0 0 0 3", "2 1 0 0 0 3")] // G
    [InlineData("x x 0 2 3 2", "0 0 0 1 3 2")] // D
    [InlineData("x 0 2 2 2 0", "0 0 1 2 3 0")] // A
    [InlineData("0 2 2 1 0 0", "0 2 3 1 0 0")] // E
    [InlineData("x 0 2 2 1 0", "0 0 2 3 1 0")] // Am
    [InlineData("x x 0 2 3 1", "0 0 0 2 3 1")] // Dm
    [InlineData("x 3 2 3 1 0", "0 3 2 4 1 0")] // C7
    [InlineData("1 3 3 2 1 1", "1 3 4 2 1 1")] // F barre
    [InlineData("x 2 4 4 3 2", "0 1 3 4 2 1")] // Bm barre
    [InlineData("0 0 0 3", "0 0 0 3")]         // C on the ukulele
    [InlineData("0 0 0 0 0 0", "0 0 0 0 0 0")]
    public void ClassicShapes(string shape, string fingers)
    {
        var positions = Parse(shape);

        Assert.Equal(fingers, string.Join(" ", Fingering.Assign(positions)!));
    }

    [Fact]
    public void BarreDoesNotCrossAnOpenString()
    {
        //six pressed strings and an open one between them cannot be held with four fingers
        Assert.Null(Fingering.Assign(new[] { 1, 2, 3, 0, 4, 5, 6 }));
    }

    private static int[] Parse(string shape) =>
        System.Array.ConvertAll(shape.Split(' '), i => i == "x" ? -1 : int.Parse(i));
}

public class FingeringOfGeneratedShapesTests
{
    [Theory]
    [InlineData(Instrument.Guitar)]
    [InlineData(Instrument.Ukulele)]
    [InlineData(Instrument.Mandolin)]
    [InlineData(Instrument.Banjo)]
    [InlineData(Instrument.BassGuitar)]
    public void MostShapesGetFingers(Instrument instrument)
    {
        var info = InstrumentInfo.Get(instrument);
        int total = 0, missing = 0;

        foreach (var root in Note.AllNotesNormalized)
        {
            foreach (var type in new[] { ChordType.Major, ChordType.Minor, ChordType.DominantSeventh, ChordType.MinorSeventh })
            {
                foreach (var layout in GuitarChordLayout.Generate(new Chord(root, type), info, true, false, false, 10))
                {
                    total++;
                    if (layout.Fingers == null)
                    {
                        missing++;
                    }
                }
            }
        }
        Assert.True(missing * 20 <= total, $"{missing} of {total} shapes have no fingering");
    }
}
