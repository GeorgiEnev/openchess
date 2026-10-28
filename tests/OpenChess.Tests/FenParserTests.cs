namespace OpenChess.Tests;

public class FenParserTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(7, 42)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Parse_CompleteFen_PreservesPlacementAndAllStateFields(int halfmoveClock, int fullmoveNumber)
    {
        string fen = FormattableString.Invariant(
            $"4k3/8/8/8/8/8/8/4K3 b Kq e3 {halfmoveClock} {fullmoveNumber}");

        Position position = FenParser.Parse(fen);

        Assert.Equal(new Piece(PieceColor.White, PieceType.King), position.GetPiece(Square.Parse("e1")));
        Assert.Equal(new Piece(PieceColor.Black, PieceType.King), position.GetPiece(Square.Parse("e8")));
        Assert.Equal(Piece.Empty, position.GetPiece(Square.Parse("e4")));
        Assert.Equal(PieceColor.Black, position.SideToMove);
        Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.BlackQueenside, position.CastlingRights);
        Assert.Equal((Square?)Square.Parse("e3"), position.EnPassantTarget);
        Assert.Equal(halfmoveClock, position.HalfmoveClock);
        Assert.Equal(fullmoveNumber, position.FullmoveNumber);
    }

    [Fact]
    public void Parse_WhitespaceAndAbsentRightsOrTarget_AreAccepted()
    {
        Position position = FenParser.Parse(" \t4k3/8/8/8/8/8/8/4K3  w\t- - 0 1\r\n");

        Assert.Equal(PieceColor.White, position.SideToMove);
        Assert.Equal(CastlingRights.None, position.CastlingRights);
        Assert.Null(position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(1, position.FullmoveNumber);
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => FenParser.Parse(null!));

        Assert.Equal("fen", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 1 extra")]
    [InlineData("8/8/8/8/8/8/8 w - - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 W - - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 x - - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w KK - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w K- - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w X - 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - z3 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - E6 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - e4 0 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - -1 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - +1 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 1.5 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 2147483648 1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 0")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 -1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 +1")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 1.5")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 abc")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 2147483648")]
    public void Parse_WithMalformedField_ThrowsFormatException(string fen)
    {
        Assert.Throws<FormatException>(() => FenParser.Parse(fen));
    }

    [Theory]
    [InlineData('P', PieceColor.White, PieceType.Pawn)]
    [InlineData('N', PieceColor.White, PieceType.Knight)]
    [InlineData('B', PieceColor.White, PieceType.Bishop)]
    [InlineData('R', PieceColor.White, PieceType.Rook)]
    [InlineData('Q', PieceColor.White, PieceType.Queen)]
    [InlineData('K', PieceColor.White, PieceType.King)]
    [InlineData('p', PieceColor.Black, PieceType.Pawn)]
    [InlineData('n', PieceColor.Black, PieceType.Knight)]
    [InlineData('b', PieceColor.Black, PieceType.Bishop)]
    [InlineData('r', PieceColor.Black, PieceType.Rook)]
    [InlineData('q', PieceColor.Black, PieceType.Queen)]
    [InlineData('k', PieceColor.Black, PieceType.King)]
    public void ParsePlacement_PieceAtEverySquare_PreservesTypeColorAndCoordinates(
        char symbol, PieceColor color, PieceType type)
    {
        Piece expected = new(color, type);

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                string[] ranks = Enumerable.Repeat("8", 8).ToArray();
                string prefix = file == 0 ? "" : file.ToString();
                string suffix = file == 7 ? "" : (7 - file).ToString();
                ranks[7 - rank] = $"{prefix}{symbol}{suffix}";

                Board board = FenParser.ParsePlacement(string.Join('/', ranks));

                for (int index = 0; index < 64; index++)
                {
                    Square square = new(index % 8, index / 8);
                    Assert.Equal(square.File == file && square.Rank == rank ? expected : Piece.Empty,
                        board.GetPiece(square));
                }

                ulong mask = 1UL << (rank * 8 + file);
                Assert.Equal(mask, board.Occupied);
                Assert.Equal(mask, board.GetPiecesByType(type));
                Assert.Equal(mask, board.GetPiecesByColor(color));
                Assert.Equal(mask, board.GetPiecesByTypeAndColor(type, color));
            }
        }
    }

    [Fact]
    public void ParsePlacement_StartingPlacement_MapsRanksAndPieceOrder()
    {
        Board board = FenParser.ParsePlacement(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        PieceType[] backRank =
        [
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        ];

        for (int file = 0; file < 8; file++)
        {
            Assert.Equal(new Piece(PieceColor.White, backRank[file]), board.GetPiece(new Square(file, 0)));
            Assert.Equal(new Piece(PieceColor.White, PieceType.Pawn), board.GetPiece(new Square(file, 1)));
            Assert.Equal(new Piece(PieceColor.Black, PieceType.Pawn), board.GetPiece(new Square(file, 6)));
            Assert.Equal(new Piece(PieceColor.Black, backRank[file]), board.GetPiece(new Square(file, 7)));

            for (int rank = 2; rank < 6; rank++)
            {
                Assert.Equal(Piece.Empty, board.GetPiece(new Square(file, rank)));
            }
        }

        Assert.Equal(0xFFFF00000000FFFFUL, board.Occupied);
        Assert.Equal(0x42UL, board.GetPiecesByTypeAndColor(PieceType.Knight, PieceColor.White));
        Assert.Equal(0x4200000000000000UL,
            board.GetPiecesByTypeAndColor(PieceType.Knight, PieceColor.Black));
    }

    [Fact]
    public void ParsePlacement_EmptyPlacement_ReturnsEmptyBoard()
    {
        Board board = FenParser.ParsePlacement("8/8/8/8/8/8/8/8");

        Assert.Equal(0UL, board.Occupied);

        for (int index = 0; index < 64; index++)
        {
            Assert.Equal(Piece.Empty, board.GetPiece(new Square(index % 8, index / 8)));
        }
    }

    [Fact]
    public void ParsePlacement_SeparateCalls_ReturnIndependentBoards()
    {
        const string placement = "4k3/8/8/8/8/8/8/4K3";
        Board first = FenParser.ParsePlacement(placement);
        Board second = FenParser.ParsePlacement(placement);
        Square e1 = Square.Parse("e1");

        first.RemovePiece(e1);

        Assert.Equal(Piece.Empty, first.GetPiece(e1));
        Assert.Equal(new Piece(PieceColor.White, PieceType.King), second.GetPiece(e1));
        Assert.Equal(0x1000000000000010UL, second.Occupied);
    }

    [Fact]
    public void ParsePlacement_WithNull_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => FenParser.ParsePlacement(null!));

        Assert.Equal("placement", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/8/8")]
    [InlineData("/8/8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/8/")]
    [InlineData("8//8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/")]
    [InlineData("7/8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/3k3")]
    [InlineData("8p/8/8/8/8/8/8/8")]
    [InlineData("7pp/8/8/8/8/8/8/8")]
    [InlineData("4k4/8/8/8/8/8/8/8")]
    [InlineData("ppppppppp/8/8/8/8/8/8/8")]
    [InlineData("44/8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/35")]
    [InlineData("0/8/8/8/8/8/8/8")]
    [InlineData("9/8/8/8/8/8/8/8")]
    [InlineData("7x/8/8/8/8/8/8/8")]
    [InlineData("7-/8/8/8/8/8/8/8")]
    [InlineData("7 /8/8/8/8/8/8/8")]
    [InlineData("7\u212A/8/8/8/8/8/8/8")]
    [InlineData("\u0668/8/8/8/8/8/8/8")]
    [InlineData("8/8/8/8/8/8/8/8 w - - 0 1")]
    public void ParsePlacement_WithMalformedInput_ThrowsFormatException(string placement)
    {
        Assert.Throws<FormatException>(() => FenParser.ParsePlacement(placement));
    }
}
