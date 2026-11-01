namespace OpenChess.Tests;

public class MoveTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(PieceType.Knight)]
    [InlineData(PieceType.Bishop)]
    [InlineData(PieceType.Rook)]
    [InlineData(PieceType.Queen)]
    public void Constructor_WithSupportedPromotionChoice_PreservesMoveFields(PieceType? promotion)
    {
        Square from = Square.Parse("a7");
        Square to = Square.Parse("a8");

        Move move = new(from, to, promotion);

        Assert.Equal(from, move.From);
        Assert.Equal(to, move.To);
        Assert.Equal(promotion, move.Promotion);
    }

    [Fact]
    public void Constructor_WithMatchingSquares_ThrowsArgumentException()
    {
        Square square = Square.Parse("d4");

        Assert.Throws<ArgumentException>(() => new Move(square, square));
    }

    [Theory]
    [InlineData((PieceType)0)]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.King)]
    [InlineData((PieceType)255)]
    public void Constructor_WithUnsupportedPromotion_ThrowsArgumentException(PieceType promotion)
    {
        Assert.Throws<ArgumentException>(
            () => new Move(Square.Parse("a7"), Square.Parse("a8"), promotion));
    }
}
