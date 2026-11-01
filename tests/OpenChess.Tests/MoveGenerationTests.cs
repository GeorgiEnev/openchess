namespace OpenChess.Tests;

public class MoveGenerationTests
{
    [Theory]
    [InlineData("8/8/8/8/3N4/8/8/8 w - - 0 1", "d4", "b3 b5 c2 c6 e2 e6 f3 f5")]
    [InlineData("8/8/8/8/8/8/8/N7 w - - 0 1", "a1", "b3 c2")]
    [InlineData("7n/8/8/8/8/8/8/8 b - - 0 1", "h8", "f7 g6")]
    [InlineData("8/8/4k3/1P3p2/3N4/8/8/8 w - - 0 1", "d4", "b3 c2 c6 e2 f3 f5")]
    [InlineData("8/8/8/8/3K4/8/8/8 w - - 0 1", "d4", "c3 c4 c5 d3 d5 e3 e4 e5")]
    [InlineData("7k/8/8/8/8/8/8/8 b - - 0 1", "h8", "g7 g8 h7")]
    [InlineData("8/8/8/2P1p3/3Kk3/8/8/8 w - - 0 1", "d4", "c3 c4 d3 d5 e3 e5")]
    public void GetPseudoLegalMovesFrom_WithOffsetPieces_RespectsPatternBoundsAndOccupants(
        string fen, string origin, string expectedDestinations)
    {
        AssertCandidates(fen, origin, expectedDestinations);
    }

    [Theory]
    [InlineData("8/8/8/8/8/8/8/R7 w - - 0 1", "a1", "b1 c1 d1 e1 f1 g1 h1 a2 a3 a4 a5 a6 a7 a8")]
    [InlineData("8/8/8/8/8/8/8/b7 b - - 0 1", "a1", "b2 c3 d4 e5 f6 g7 h8")]
    [InlineData("8/8/8/8/3Q4/8/8/8 w - - 0 1", "d4",
        "a4 b4 c4 e4 f4 g4 h4 d1 d2 d3 d5 d6 d7 d8 a1 b2 c3 e5 f6 g7 h8 a7 b6 c5 e3 f2 g1")]
    [InlineData("8/8/3p4/8/1k1R1P2/8/3P4/8 w - - 7 42", "d4", "c4 e4 d3 d5 d6")]
    [InlineData("8/8/1p3P2/8/3B4/8/1P3p2/8 w - - 0 1", "d4", "c3 c5 b6 e3 f2 e5")]
    [InlineData("8/8/1p1p1P2/8/1k1Q1P2/8/1P1P1p2/8 w - - 7 42", "d4",
        "c4 e4 d3 d5 d6 c3 c5 b6 e3 f2 e5")]
    public void GetPseudoLegalMovesFrom_WithSlidingPieces_StopsAtEdgesAndFirstOccupants(
        string fen, string origin, string expectedDestinations)
    {
        AssertCandidates(fen, origin, expectedDestinations);
    }

    [Theory]
    [InlineData("4r3/8/8/8/8/8/4R3/4K3 w - - 0 1", "e2",
        "a2 b2 c2 d2 f2 g2 h2 e3 e4 e5 e6 e7 e8")]
    [InlineData("4r3/8/8/8/8/8/8/R3K2R w KQ - 0 1", "e1", "d1 f1 d2 e2 f2")]
    public void GetPseudoLegalMovesFrom_WithoutKingSafetyFiltering_ReturnsCandidatesButNoCastling(
        string fen, string origin, string expectedDestinations)
    {
        AssertCandidates(fen, origin, expectedDestinations);
    }

    [Fact]
    public void GetPseudoLegalMovesFrom_WithRepeatedQueries_KeepsResultsIndependentAndPositionUnchanged()
    {
        const string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        Position position = Position.ParseFen(fen);

        IReadOnlyList<Move> first = position.GetPseudoLegalMovesFrom(Square.Parse("g1"));
        IReadOnlyList<Move> other = position.GetPseudoLegalMovesFrom(Square.Parse("b1"));
        IReadOnlyList<Move> repeated = position.GetPseudoLegalMovesFrom(Square.Parse("g1"));

        Assert.Equal(new[] { "f3", "h3" }, first.Select(move => move.To.ToString()).OrderBy(value => value));
        Assert.Equal(new[] { "a3", "c3" }, other.Select(move => move.To.ToString()).OrderBy(value => value));
        Assert.Equal(new[] { "f3", "h3" }, repeated.Select(move => move.To.ToString()).OrderBy(value => value));
        Assert.Equal(fen, position.ToFen());
    }

    [Fact]
    public void Generate_WithNullPosition_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => MoveGenerator.Generate(null!, Square.Parse("a1")));

        Assert.Equal("position", exception.ParamName);
    }

    private static void AssertCandidates(string fen, string origin, string expectedDestinations)
    {
        Position position = Position.ParseFen(fen);
        Square from = Square.Parse(origin);

        IReadOnlyList<Move> moves = position.GetPseudoLegalMovesFrom(from);

        Assert.All(moves, move =>
        {
            Assert.Equal(from, move.From);
            Assert.Null(move.Promotion);
        });
        Assert.Equal(expectedDestinations.Split(' ').OrderBy(value => value),
            moves.Select(move => move.To.ToString()).OrderBy(value => value));
        Assert.Equal(fen, position.ToFen());
    }
}
