namespace OpenChess.Tests;

public class PositionTests
{
    [Fact]
    public void ParseFen_WithStartingFen_ExposesStartingPlacementAndState()
    {
        Position position = Position.ParseFen(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        Assert.Equal(new Piece(PieceColor.White, PieceType.King), position.GetPiece(Square.Parse("e1")));
        Assert.Equal(new Piece(PieceColor.Black, PieceType.Rook), position.GetPiece(Square.Parse("h8")));
        Assert.Equal(PieceColor.White, position.SideToMove);
        Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside |
            CastlingRights.BlackKingside | CastlingRights.BlackQueenside, position.CastlingRights);
        Assert.Null(position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(1, position.FullmoveNumber);
    }

    [Fact]
    public void ParseFen_WithCompleteFen_ExposesPlacementAndState()
    {
        Position position = Position.ParseFen("r3k2r/8/8/8/4P3/8/8/R3K2R b Kq e3 0 42");

        Assert.Equal(new Piece(PieceColor.White, PieceType.Pawn), position.GetPiece(Square.Parse("e4")));
        Assert.Equal(new Piece(PieceColor.Black, PieceType.King), position.GetPiece(Square.Parse("e8")));
        Assert.Equal(Piece.Empty, position.GetPiece(Square.Parse("e2")));
        Assert.Equal(PieceColor.Black, position.SideToMove);
        Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.BlackQueenside, position.CastlingRights);
        Assert.Equal((Square?)Square.Parse("e3"), position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(42, position.FullmoveNumber);
    }

    [Fact]
    public void ParseFen_WithNull_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => Position.ParseFen(null!));

        Assert.Equal("fen", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 0")]
    public void ParseFen_WithMalformedInput_ThrowsFormatException(string fen)
    {
        Assert.Throws<FormatException>(() => Position.ParseFen(fen));
    }

    [Theory]
    [InlineData(PieceColor.White, null, 0, 1)]
    [InlineData(PieceColor.Black, "a3", int.MaxValue, int.MaxValue)]
    [InlineData(PieceColor.White, "h6", 7, 42)]
    public void Constructor_WithValidState_PreservesPlacementAndFields(
        PieceColor sideToMove, string? target, int halfmoveClock, int fullmoveNumber)
    {
        Board board = new();
        Square square = Square.Parse("c3");
        Piece piece = new(PieceColor.Black, PieceType.Knight);
        board.PlacePiece(square, piece);
        Square? enPassantTarget = target is null ? null : Square.Parse(target);

        Position position = new(board, sideToMove, CastlingRights.None,
            enPassantTarget, halfmoveClock, fullmoveNumber);

        Assert.Equal(piece, position.GetPiece(square));
        Assert.Equal(sideToMove, position.SideToMove);
        Assert.Equal(CastlingRights.None, position.CastlingRights);
        Assert.Equal(enPassantTarget, position.EnPassantTarget);
        Assert.Equal(halfmoveClock, position.HalfmoveClock);
        Assert.Equal(fullmoveNumber, position.FullmoveNumber);
    }

    [Fact]
    public void Constructor_WithSupportedCastlingCombinations_AcceptsEveryCombination()
    {
        for (int bits = 0; bits < 16; bits++)
        {
            CastlingRights rights = (CastlingRights)bits;

            Position position = new(new Board(), PieceColor.White, rights, null, 0, 1);

            Assert.Equal(rights, position.CastlingRights);
        }
    }

    [Fact]
    public void Constructor_WithNullBoard_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new Position(null!, PieceColor.White, CastlingRights.None, null, 0, 1));

        Assert.Equal("board", exception.ParamName);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(255)]
    public void Constructor_WithInvalidSide_ThrowsArgumentOutOfRangeException(int side)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Position(new Board(), (PieceColor)side, CastlingRights.None, null, 0, 1));

        Assert.Equal("sideToMove", exception.ParamName);
        Assert.Equal((PieceColor)side, exception.ActualValue);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(255)]
    public void Constructor_WithUnsupportedCastlingBits_ThrowsArgumentOutOfRangeException(int rights)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Position(new Board(), PieceColor.White, (CastlingRights)rights, null, 0, 1));

        Assert.Equal("castlingRights", exception.ParamName);
        Assert.Equal((CastlingRights)rights, exception.ActualValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(7)]
    public void Constructor_WithUnsupportedTargetRank_ThrowsArgumentOutOfRangeException(int rank)
    {
        Square target = new(4, rank);

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Position(new Board(), PieceColor.White, CastlingRights.None, target, 0, 1));

        Assert.Equal("enPassantTarget", exception.ParamName);
        Assert.Equal(target, exception.ActualValue);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_WithNegativeHalfmoveClock_ThrowsArgumentOutOfRangeException(int clock)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Position(new Board(), PieceColor.White, CastlingRights.None, null, clock, 1));

        Assert.Equal("halfmoveClock", exception.ParamName);
        Assert.Equal(clock, exception.ActualValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_WithNonpositiveFullmoveNumber_ThrowsArgumentOutOfRangeException(int number)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Position(new Board(), PieceColor.White, CastlingRights.None, null, 0, number));

        Assert.Equal("fullmoveNumber", exception.ParamName);
        Assert.Equal(number, exception.ActualValue);
    }
}
