namespace OpenChess.Tests;

public class PawnMoveGenerationTests
{
    [Theory]
    [InlineData("8/8/8/8/8/8/4P3/8 w - - 0 1", "e2", new[] { "e3", "e4" })]
    [InlineData("8/4p3/8/8/8/8/8/8 b - - 0 1", "e7", new[] { "e6", "e5" })]
    [InlineData("8/8/8/8/4P3/8/8/8 w - - 0 1", "e4", new[] { "e5" })]
    [InlineData("8/8/8/4p3/8/8/8/8 b - - 0 1", "e5", new[] { "e4" })]
    [InlineData("8/8/8/8/8/4N3/4P3/8 w - - 0 1", "e2", new string[0])]
    [InlineData("8/8/8/8/8/4n3/4P3/8 w - - 0 1", "e2", new string[0])]
    [InlineData("8/8/8/8/4n3/8/4P3/8 w - - 0 1", "e2", new[] { "e3" })]
    [InlineData("8/4p3/4N3/8/8/8/8/8 b - - 0 1", "e7", new string[0])]
    [InlineData("8/4p3/8/4N3/8/8/8/8 b - - 0 1", "e7", new[] { "e6" })]
    public void GetPseudoLegalMovesFrom_WithPawnAdvances_RequiresClearSquares(
        string fen, string origin, string[] expected)
    {
        AssertCandidates(fen, origin, expected);
    }

    [Theory]
    [InlineData("8/8/8/3n1b2/4P3/8/8/8 w - - 0 1", "e4", new[] { "d5", "e5", "f5" })]
    [InlineData("8/8/8/4p3/3N1B2/8/8/8 b - - 0 1", "e5", new[] { "d4", "e4", "f4" })]
    [InlineData("8/8/8/3N1k2/4P3/8/8/8 w - - 0 1", "e4", new[] { "e5" })]
    [InlineData("8/8/8/1p6/P7/8/8/8 w - - 0 1", "a4", new[] { "a5", "b5" })]
    [InlineData("8/8/8/7p/6P1/8/8/8 b - - 0 1", "h5", new[] { "h4", "g4" })]
    [InlineData("P7/8/8/8/8/8/8/8 w - - 0 1", "a8", new string[0])]
    [InlineData("8/8/8/8/8/8/8/7p b - - 0 1", "h1", new string[0])]
    public void GetPseudoLegalMovesFrom_WithPawnCapturesAndEdges_RespectsOccupancyAndBounds(
        string fen, string origin, string[] expected)
    {
        AssertCandidates(fen, origin, expected);
    }

    [Theory]
    [InlineData("1r6/P7/8/8/8/8/8/8 w - - 0 1", "a7", new[] { "a8", "b8" })]
    [InlineData("8/8/8/8/8/8/7p/6R1 b - - 0 1", "h2", new[] { "g1", "h1" })]
    [InlineData("1r6/1P6/8/8/8/8/8/8 w - - 0 1", "b7", new string[0])]
    public void GetPseudoLegalMovesFrom_OnPromotionRank_ReturnsFourChoicesPerReachableDestination(
        string fen, string origin, string[] destinations)
    {
        PieceType[] promotions = [PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight];
        string[] expected = destinations
            .SelectMany(destination => promotions.Select(promotion => $"{destination}/{promotion}"))
            .ToArray();

        AssertCandidates(fen, origin, expected);
    }

    [Theory]
    [InlineData("8/8/8/3pP3/8/8/8/8 w - d6 0 1", "e5", new[] { "e6", "d6" })]
    [InlineData("8/8/8/8/3Pp3/8/8/8 b - d3 0 1", "e4", new[] { "e3", "d3" })]
    [InlineData("8/8/8/pP6/8/8/8/8 w - a6 0 1", "b5", new[] { "a6", "b6" })]
    [InlineData("8/8/8/8/6pP/8/8/8 b - h3 0 1", "g4", new[] { "g3", "h3" })]
    [InlineData("8/8/8/3pP3/8/8/8/8 w - - 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/8/4P3/8/8/8/8 w - d6 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/8/3PP3/8/8/8/8 w - d6 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/8/3rP3/8/8/8/8 w - d6 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/3N4/3pP3/8/8/8/8 w - d6 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/8/3pP3/8/8/8/8 w - a6 0 1", "e5", new[] { "e6" })]
    [InlineData("8/8/8/8/8/3pP3/8/8 w - d3 0 1", "e3", new[] { "e4" })]
    [InlineData("8/8/8/K2pP2r/8/8/8/8 w - d6 0 1", "e5", new[] { "e6", "d6" })]
    public void GetPseudoLegalMovesFrom_WithEnPassant_RequiresMatchingTargetAndPawnPlacement(
        string fen, string origin, string[] expected)
    {
        AssertCandidates(fen, origin, expected);
    }

    [Theory]
    [InlineData("8/8/8/8/8/8/4P3/8 b - - 0 1", "e2")]
    [InlineData("8/8/8/8/8/8/4P3/8 w - - 0 1", "d2")]
    public void GetPseudoLegalMovesFrom_WithEmptyOriginOrWrongSide_ReturnsNoCandidates(
        string fen, string origin)
    {
        AssertCandidates(fen, origin, []);
    }

    private static void AssertCandidates(string fen, string origin, string[] expected)
    {
        Position position = Position.ParseFen(fen);
        Square from = Square.Parse(origin);

        IReadOnlyList<Move> moves = position.GetPseudoLegalMovesFrom(from);

        Assert.All(moves, move => Assert.Equal(from, move.From));
        string[] actual = moves.Select(move => move.Promotion is PieceType promotion
            ? $"{move.To}/{promotion}"
            : move.To.ToString()).ToArray();
        Assert.Equal(expected.OrderBy(value => value), actual.OrderBy(value => value));
        Assert.Equal(fen, position.ToFen());
    }
}
