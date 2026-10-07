namespace OpenChess.Tests;

public class SquareTests
{
    public static IEnumerable<object[]> SquareCoordinates()
    {
        const string files = "abcdefgh";
        const string ranks = "12345678";
        int index = 0;

        for (int rank = 0; rank < ranks.Length; rank++)
        {
            for (int file = 0; file < files.Length; file++)
            {
                yield return [file, rank, index++, $"{files[file]}{ranks[rank]}"];
            }
        }
    }

    public static IEnumerable<object[]> InvalidCoordinates()
    {
        string[] inputs =
        [
            "", "a", "a11", "A1", "i1", "`1", "a0", "a9", "1a",
            " a1", "a1 ", "a\0", "\u00E91", "a\uFF11"
        ];

        foreach (string input in inputs)
        {
            yield return [input];
        }
    }

    [Theory]
    [MemberData(nameof(SquareCoordinates))]
    public void ValidCoordinates_MapToExpectedIndexAndNotation(
        int file, int rank, int expectedIndex, string coordinate)
    {
        Square square = new(file, rank);

        Assert.Equal(file, square.File);
        Assert.Equal(rank, square.Rank);
        Assert.Equal(expectedIndex, square.Index);
        Assert.Equal(coordinate, square.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Constructor_WithInvalidFile_ThrowsArgumentOutOfRangeException(int file)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Square(file, 0));

        Assert.Equal("file", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Constructor_WithInvalidRank_ThrowsArgumentOutOfRangeException(int rank)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Square(0, rank));

        Assert.Equal("rank", exception.ParamName);
    }

    [Fact]
    public void Default_RepresentsA1()
    {
        Square square = default;

        Assert.Equal(0, square.Index);
        Assert.Equal(0, square.File);
        Assert.Equal(0, square.Rank);
        Assert.Equal(new Square(0, 0), square);
        Assert.Equal("a1", square.ToString());
    }

    [Theory]
    [MemberData(nameof(SquareCoordinates))]
    public void Parsing_WithValidCoordinate_ReturnsExpectedSquare(
        int file, int rank, int expectedIndex, string coordinate)
    {
        bool success = Square.TryParse(coordinate, out Square parsed);

        Assert.True(success);
        Assert.Equal(file, parsed.File);
        Assert.Equal(rank, parsed.Rank);
        Assert.Equal(expectedIndex, parsed.Index);
        Assert.Equal(parsed, Square.Parse(coordinate));
    }

    [Theory]
    [MemberData(nameof(InvalidCoordinates))]
    public void TryParse_WithInvalidText_ReturnsFalseAndResetsOutput(string text)
    {
        Square square = new(7, 7);

        bool success = Square.TryParse(text, out square);

        Assert.False(success);
        Assert.Equal(default, square);
    }

    [Fact]
    public void TryParse_WithNull_ReturnsFalseAndResetsOutput()
    {
        Square square = new(7, 7);

        bool success = Square.TryParse(null, out square);

        Assert.False(success);
        Assert.Equal(default, square);
    }

    [Theory]
    [MemberData(nameof(InvalidCoordinates))]
    public void Parse_WithInvalidText_ThrowsFormatException(string text)
    {
        Assert.Throws<FormatException>(() => Square.Parse(text));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => Square.Parse(null!));

        Assert.Equal("text", exception.ParamName);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, true)]
    [InlineData(4, 3, 4, 3, true)]
    [InlineData(7, 7, 7, 7, true)]
    [InlineData(4, 3, 5, 3, false)]
    [InlineData(4, 3, 4, 4, false)]
    [InlineData(1, 0, 0, 1, false)]
    [InlineData(0, 0, 7, 7, false)]
    public void Equality_AgreesAcrossComparisonMethods(
        int leftFile, int leftRank, int rightFile, int rightRank, bool expected)
    {
        Square left = new(leftFile, leftRank);
        Square right = new(rightFile, rightRank);

        Assert.Equal(expected, left.Equals(right));
        Assert.Equal(expected, right.Equals(left));
        Assert.Equal(expected, left.Equals((object)right));
        Assert.Equal(expected, ((IEquatable<Square>)left).Equals(right));
        Assert.Equal(expected, left == right);
        Assert.Equal(!expected, left != right);
        Assert.Equal(expected, EqualityComparer<Square>.Default.Equals(left, right));
    }

    [Fact]
    public void Equals_WithNullOrOtherObjectType_ReturnsFalse()
    {
        Square square = new(4, 3);

        Assert.False(square.Equals((object?)null));
        Assert.False(square.Equals(new object()));
        Assert.False(square.Equals(square.Index));
    }

    [Fact]
    public void GetHashCode_EqualSquares_ReturnSameHash()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Square first = new(file, rank);
                Square equal = new(file, rank);

                Assert.Equal(first.GetHashCode(), equal.GetHashCode());
            }
        }
    }

    [Fact]
    public void HashSet_KeepsAllSquaresAndRejectsDuplicates()
    {
        HashSet<Square> squares = new();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Assert.True(squares.Add(new Square(file, rank)));
                Assert.False(squares.Add(new Square(file, rank)));
            }
        }

        Assert.Equal(64, squares.Count);
    }
}
