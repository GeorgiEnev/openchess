namespace OpenChess.Tests;

public class FenSerializerTests
{
    [Theory]
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
    [InlineData("8/8/2N3k1/8/8/8/8/8 b q e3 7 42")]
    [InlineData("8/8/8/8/8/8/8/8 w k - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 b - h6 2147483647 2147483647")]
    public void ToFen_WithCanonicalFen_PreservesPlacementAndState(string fen)
    {
        Position position = Position.ParseFen(fen);

        Assert.Equal(fen, position.ToFen());
    }

    [Fact]
    public void ToFen_WithUnorderedRightsAndExtraWhitespace_WritesCanonicalFen()
    {
        Position position = Position.ParseFen("  8/8/8/8/8/8/8/8\tw qkQK - 0 1  ");

        Assert.Equal("8/8/8/8/8/8/8/8 w KQkq - 0 1", position.ToFen());
    }

    [Fact]
    public void ToFen_WithExplicitPlacement_WritesEveryPieceLetterInBoardOrder()
    {
        Board board = new();
        PieceType[] types =
        [
            PieceType.Pawn, PieceType.Knight, PieceType.Bishop,
            PieceType.Rook, PieceType.Queen, PieceType.King
        ];

        for (int file = 0; file < types.Length; file++)
        {
            board.PlacePiece(new Square(file, 7), new Piece(PieceColor.Black, types[file]));
            board.PlacePiece(new Square(file + 2, 0), new Piece(PieceColor.White, types[file]));
        }

        Position position = new(board, PieceColor.White, CastlingRights.None, null, 0, 1);

        Assert.Equal("pnbrqk2/8/8/8/8/8/8/2PNBRQK w - - 0 1", position.ToFen());
    }

    [Theory]
    [InlineData("P7/7p/8/8/8/8/8/8")]
    [InlineData("8/8/8/1P1P1P2/8/8/8/8")]
    [InlineData("8/8/8/PpPpPpPp/8/8/8/8")]
    public void ToFen_WithDifferentEmptyRuns_WritesCompressedRanks(string placement)
    {
        string fen = placement + " w - - 0 1";
        Position position = Position.ParseFen(fen);

        Assert.Equal(fen, position.ToFen());
    }

    [Theory]
    [InlineData(0, "-")]
    [InlineData(1, "K")]
    [InlineData(2, "Q")]
    [InlineData(3, "KQ")]
    [InlineData(4, "k")]
    [InlineData(5, "Kk")]
    [InlineData(6, "Qk")]
    [InlineData(7, "KQk")]
    [InlineData(8, "q")]
    [InlineData(9, "Kq")]
    [InlineData(10, "Qq")]
    [InlineData(11, "KQq")]
    [InlineData(12, "kq")]
    [InlineData(13, "Kkq")]
    [InlineData(14, "Qkq")]
    [InlineData(15, "KQkq")]
    public void ToFen_WithCastlingFlags_WritesOnlyPresentRightsInCanonicalOrder(
        int flags, string expectedRights)
    {
        Position position = new(new Board(), PieceColor.White,
            (CastlingRights)flags, null, 0, 1);

        Assert.Equal($"8/8/8/8/8/8/8/8 w {expectedRights} - 0 1", position.ToFen());
    }

    [Fact]
    public void ToFen_WhenParsedAgain_PreservesStateAndLeavesSourceUnchanged()
    {
        Position original = Position.ParseFen("r3k2r/1p4p1/8/3pP3/8/2N5/8/R3K2R w Kq d6 7 42");
        Piece[] piecesBefore = new Piece[64];
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                piecesBefore[rank * 8 + file] = original.GetPiece(new Square(file, rank));
            }
        }

        string fen = original.ToFen();
        Position restored = Position.ParseFen(fen);

        foreach (Position position in new[] { original, restored })
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Assert.Equal(piecesBefore[rank * 8 + file], position.GetPiece(new Square(file, rank)));
                }
            }

            Assert.Equal(PieceColor.White, position.SideToMove);
            Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.BlackQueenside, position.CastlingRights);
            Assert.Equal((Square?)Square.Parse("d6"), position.EnPassantTarget);
            Assert.Equal(7, position.HalfmoveClock);
            Assert.Equal(42, position.FullmoveNumber);
        }
    }

    [Fact]
    public void Serialize_WithNullPosition_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => FenSerializer.Serialize(null!));

        Assert.Equal("position", exception.ParamName);
    }
}
