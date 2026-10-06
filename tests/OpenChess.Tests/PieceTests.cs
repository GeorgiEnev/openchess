namespace OpenChess.Tests;

public class PieceTests
{
    [Theory]
    [InlineData(PieceColor.White, PieceType.Pawn)]
    [InlineData(PieceColor.White, PieceType.Knight)]
    [InlineData(PieceColor.White, PieceType.Bishop)]
    [InlineData(PieceColor.White, PieceType.Rook)]
    [InlineData(PieceColor.White, PieceType.Queen)]
    [InlineData(PieceColor.White, PieceType.King)]
    [InlineData(PieceColor.Black, PieceType.Pawn)]
    [InlineData(PieceColor.Black, PieceType.Knight)]
    [InlineData(PieceColor.Black, PieceType.Bishop)]
    [InlineData(PieceColor.Black, PieceType.Rook)]
    [InlineData(PieceColor.Black, PieceType.Queen)]
    [InlineData(PieceColor.Black, PieceType.King)]
    public void Constructor_WithValidArguments_PreservesColorAndType(
        PieceColor color,
        PieceType type)
    {
        Piece piece = new(color, type);

        Assert.Equal(color, piece.Color);
        Assert.Equal(type, piece.Type);
        Assert.False(piece.IsEmpty);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(8)]
    [InlineData(255)]
    public void Constructor_WithInvalidColor_ThrowsArgumentException(int color)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Piece((PieceColor)color, PieceType.Knight));

        Assert.Equal("color", exception.ParamName);
    }

    [Theory]
    [InlineData(PieceColor.White, 0)]
    [InlineData(PieceColor.White, 7)]
    [InlineData(PieceColor.White, 8)]
    [InlineData(PieceColor.White, 255)]
    [InlineData(PieceColor.Black, 0)]
    [InlineData(PieceColor.Black, 7)]
    [InlineData(PieceColor.Black, 8)]
    [InlineData(PieceColor.Black, 255)]
    public void Constructor_WithInvalidType_ThrowsArgumentException(
        PieceColor color,
        int type)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new Piece(color, (PieceType)type));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Empty_EqualsDefaultAndParameterlessValues()
    {
        Piece empty = Piece.Empty;
        Piece defaultPiece = default;
        Piece parameterlessPiece = new();

        Assert.True(empty.IsEmpty);
        Assert.True(defaultPiece.IsEmpty);
        Assert.True(parameterlessPiece.IsEmpty);
        Assert.Equal(empty, defaultPiece);
        Assert.Equal(empty, parameterlessPiece);
    }

    [Fact]
    public void Empty_Color_ThrowsInvalidOperationException()
    {
        Piece piece = Piece.Empty;

        Assert.Throws<InvalidOperationException>(() => piece.Color);
    }

    [Fact]
    public void Empty_Type_ThrowsInvalidOperationException()
    {
        Piece piece = Piece.Empty;

        Assert.Throws<InvalidOperationException>(() => piece.Type);
    }

    [Fact]
    public void Equality_AllValuePairs_AgreesAcrossComparisonMethods()
    {
        Piece[] values = GetAllPieceValues();

        for (int leftIndex = 0; leftIndex < values.Length; leftIndex++)
        {
            for (int rightIndex = 0; rightIndex < values.Length; rightIndex++)
            {
                Piece left = values[leftIndex];
                Piece right = values[rightIndex];
                bool expected = leftIndex == rightIndex;

                Assert.Equal(expected, left.Equals(right));
                Assert.Equal(expected, left.Equals((object)right));
                Assert.Equal(expected, ((IEquatable<Piece>)left).Equals(right));
                Assert.Equal(expected, left == right);
                Assert.Equal(!expected, left != right);
                Assert.Equal(expected, EqualityComparer<Piece>.Default.Equals(left, right));
            }
        }
    }

    [Fact]
    public void Equals_WithNullOrOtherObjectType_ReturnsFalse()
    {
        foreach (Piece piece in GetAllPieceValues())
        {
            Assert.False(piece.Equals((object?)null));
            Assert.False(piece.Equals(new object()));
            Assert.False(piece.Equals(0));
        }
    }

    [Fact]
    public void GetHashCode_EqualValues_ReturnSameHash()
    {
        foreach (Piece piece in GetAllPieceValues())
        {
            Piece equal = piece.IsEmpty ? default : new Piece(piece.Color, piece.Type);

            Assert.True(piece.Equals(equal));
            Assert.Equal(piece.GetHashCode(), equal.GetHashCode());
        }
    }

    [Fact]
    public void HashSet_DeduplicatesEqualValuesAndKeepsDistinctValues()
    {
        Piece[] values = GetAllPieceValues();
        HashSet<Piece> pieces = new();

        foreach (Piece piece in values)
        {
            Assert.True(pieces.Add(piece));
            Piece equal = piece.IsEmpty ? default : new Piece(piece.Color, piece.Type);
            Assert.False(pieces.Add(equal));
        }

        Assert.Equal(13, pieces.Count);
    }

    private static Piece[] GetAllPieceValues()
    {
        List<Piece> pieces = [Piece.Empty];

        foreach (PieceColor color in Enum.GetValues<PieceColor>())
        {
            foreach (PieceType type in Enum.GetValues<PieceType>())
            {
                pieces.Add(new Piece(color, type));
            }
        }

        return pieces.ToArray();
    }
}
