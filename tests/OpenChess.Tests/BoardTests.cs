namespace OpenChess.Tests;

public class BoardTests
{
    public static IEnumerable<object[]> PieceCombinations()
    {
        foreach (PieceColor color in Enum.GetValues<PieceColor>())
        {
            foreach (PieceType type in Enum.GetValues<PieceType>())
            {
                yield return [color, type];
            }
        }
    }

    [Fact]
    public void NewBoard_HasNoPiecesOrOccupiedSquares()
    {
        Board board = new();

        AssertPlacement(board, new Piece[64]);
    }

    [Theory]
    [MemberData(nameof(PieceCombinations))]
    public void PlacementAndRemoval_AllSquares_UpdateMatchingBitboards(
        PieceColor color, PieceType type)
    {
        Board board = new();
        Piece piece = new(color, type);
        Piece[] expected = new Piece[64];

        for (int index = 0; index < expected.Length; index++)
        {
            Square square = new(index % 8, index / 8);

            board.PlacePiece(square, piece);
            expected[index] = piece;

            AssertPlacement(board, expected);

            board.RemovePiece(square);
            expected[index] = Piece.Empty;

            AssertPlacement(board, expected);
        }
    }

    [Fact]
    public void MultiplePlacementsAndRemovals_PreserveOtherPiecesAndFilterQueries()
    {
        Board board = new();
        Piece[] expected = new Piece[64];
        PieceType[] types = Enum.GetValues<PieceType>();

        for (int index = 0; index < expected.Length; index++)
        {
            PieceColor color = index % 12 < 6 ? PieceColor.White : PieceColor.Black;
            Piece piece = new(color, types[index % types.Length]);
            Square square = new(index % 8, index / 8);

            board.PlacePiece(square, piece);
            expected[index] = piece;

            AssertPlacement(board, expected);
        }

        Assert.Equal(ulong.MaxValue, board.Occupied);

        // Remove alternate squares first so each removal is checked against remaining pieces.
        for (int parity = 0; parity < 2; parity++)
        {
            for (int index = parity; index < expected.Length; index += 2)
            {
                board.RemovePiece(new Square(index % 8, index / 8));
                expected[index] = Piece.Empty;

                AssertPlacement(board, expected);
            }
        }
    }

    [Fact]
    public void RemovePiece_FromEmptySquare_LeavesPlacementUnchanged()
    {
        Board board = new();
        Piece[] expected = new Piece[64];
        Piece piece = new(PieceColor.Black, PieceType.Rook);
        Square occupied = Square.Parse("h8");
        board.PlacePiece(occupied, piece);
        expected[63] = piece;

        board.RemovePiece(Square.Parse("a1"));

        AssertPlacement(board, expected);
    }

    [Fact]
    public void RemovedSquare_CanBeReusedByDifferentTypeAndColor()
    {
        Board board = new();
        Square square = Square.Parse("c3");
        board.PlacePiece(square, new Piece(PieceColor.White, PieceType.Knight));
        board.RemovePiece(square);
        Piece replacement = new(PieceColor.Black, PieceType.Bishop);

        board.PlacePiece(square, replacement);

        Piece[] expected = new Piece[64];
        expected[18] = replacement;
        AssertPlacement(board, expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlacePiece_WithEmptyPiece_ThrowsAndLeavesPlacementUnchanged(bool occupied)
    {
        Board board = new();
        Piece[] expected = new Piece[64];
        Square destination = Square.Parse("c3");
        Piece otherPiece = new(PieceColor.Black, PieceType.Rook);
        board.PlacePiece(Square.Parse("h8"), otherPiece);
        expected[63] = otherPiece;

        if (occupied)
        {
            Piece piece = new(PieceColor.White, PieceType.Knight);
            board.PlacePiece(destination, piece);
            expected[18] = piece;
        }

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => board.PlacePiece(destination, Piece.Empty));

        Assert.Equal("piece", exception.ParamName);
        AssertPlacement(board, expected);
    }

    [Theory]
    [InlineData(PieceColor.White, PieceType.Knight)]
    [InlineData(PieceColor.White, PieceType.Bishop)]
    [InlineData(PieceColor.Black, PieceType.Knight)]
    [InlineData(PieceColor.Black, PieceType.Queen)]
    public void PlacePiece_OnOccupiedSquare_ThrowsAndPreservesExistingPlacement(
        PieceColor color, PieceType type)
    {
        Board board = new();
        Piece[] expected = new Piece[64];
        Square destination = Square.Parse("c3");
        Piece existing = new(PieceColor.White, PieceType.Knight);
        Piece otherPiece = new(PieceColor.Black, PieceType.Rook);
        board.PlacePiece(destination, existing);
        board.PlacePiece(Square.Parse("h8"), otherPiece);
        expected[18] = existing;
        expected[63] = otherPiece;
        Piece supplied = new(color, type);

        Assert.Throws<InvalidOperationException>(() => board.PlacePiece(destination, supplied));

        AssertPlacement(board, expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(255)]
    public void TypeQueries_WithInvalidType_ThrowArgumentOutOfRangeException(int type)
    {
        Board board = new();

        ArgumentOutOfRangeException typeException = Assert.Throws<ArgumentOutOfRangeException>(
            () => board.GetPiecesByType((PieceType)type));
        Assert.Equal("type", typeException.ParamName);
        Assert.Equal((PieceType)type, typeException.ActualValue);

        foreach (PieceColor color in Enum.GetValues<PieceColor>())
        {
            ArgumentOutOfRangeException combinedException = Assert.Throws<ArgumentOutOfRangeException>(
                () => board.GetPiecesByTypeAndColor((PieceType)type, color));
            Assert.Equal("type", combinedException.ParamName);
            Assert.Equal((PieceType)type, combinedException.ActualValue);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(255)]
    public void ColorQueries_WithInvalidColor_ThrowArgumentOutOfRangeException(int color)
    {
        Board board = new();

        ArgumentOutOfRangeException colorException = Assert.Throws<ArgumentOutOfRangeException>(
            () => board.GetPiecesByColor((PieceColor)color));
        Assert.Equal("color", colorException.ParamName);
        Assert.Equal((PieceColor)color, colorException.ActualValue);

        foreach (PieceType type in Enum.GetValues<PieceType>())
        {
            ArgumentOutOfRangeException combinedException = Assert.Throws<ArgumentOutOfRangeException>(
                () => board.GetPiecesByTypeAndColor(type, (PieceColor)color));
            Assert.Equal("color", combinedException.ParamName);
            Assert.Equal((PieceColor)color, combinedException.ActualValue);
        }
    }

    private static void AssertPlacement(Board board, Piece[] expected)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], board.GetPiece(new Square(index % 8, index / 8)));
        }

        foreach (PieceType type in Enum.GetValues<PieceType>())
        {
            Assert.Equal(GetExpectedSquares(expected, piece => piece.Type == type),
                board.GetPiecesByType(type));

            foreach (PieceColor color in Enum.GetValues<PieceColor>())
            {
                Assert.Equal(GetExpectedSquares(expected, piece => piece.Type == type && piece.Color == color),
                    board.GetPiecesByTypeAndColor(type, color));
            }
        }

        foreach (PieceColor color in Enum.GetValues<PieceColor>())
        {
            Assert.Equal(GetExpectedSquares(expected, piece => piece.Color == color),
                board.GetPiecesByColor(color));
        }

        Assert.Equal(GetExpectedSquares(expected, _ => true), board.Occupied);
    }

    private static ulong GetExpectedSquares(Piece[] expected, Func<Piece, bool> matches)
    {
        ulong result = 0;
        ulong squareBit = 1;

        foreach (Piece piece in expected)
        {
            if (!piece.IsEmpty && matches(piece))
            {
                result |= squareBit;
            }

            squareBit <<= 1;
        }

        return result;
    }
}
