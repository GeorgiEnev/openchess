namespace OpenChess;

/// <summary>
/// Represents a chess piece, with a special empty value for an unoccupied square.
/// </summary>
/// <remarks>
/// A piece stores its color and type.
/// <para>
/// <c>default(Piece)</c> is equal to <see cref="Empty"/>. An empty value has no color
/// or type, so check <see cref="IsEmpty"/> before reading <see cref="Color"/>
/// or <see cref="Type"/>.
/// </para>
/// </remarks>
public readonly struct Piece : IEquatable<Piece>
{
    // Bits 0-2 hold the type; bit 3 holds the color.
    private readonly byte value;

    /// <summary>
    /// Creates a piece with the given color and type.
    /// </summary>
    /// <param name="color">The piece's color: White or Black.</param>
    /// <param name="type">The piece's type: Pawn, Knight, Bishop, Rook, Queen, or King.</param>
    /// <exception cref="ArgumentException">
    /// The color is not White or Black, or the type is not one of the six piece types.
    /// </exception>
    public Piece(PieceColor color, PieceType type)
    {
        if (color is not (PieceColor.White or PieceColor.Black))
        {
            throw new ArgumentException(
                "Color must be White or Black.",
                nameof(color));
        }

        if ((byte)type < (byte)PieceType.Pawn ||
            (byte)type > (byte)PieceType.King)
        {
            throw new ArgumentException(
                "Type must be a supported chess piece type.",
                nameof(type));
        }

        value = (byte)((byte)type | (byte)color << 3);
    }

    /// <summary>
    /// Gets whether this value represents an empty square.
    /// </summary>
    public bool IsEmpty => value == 0;

    /// <summary>
    /// Gets the value used when a board square contains no piece.
    /// </summary>
    /// <remarks>
    /// Equivalent to <c>default(Piece)</c>. Its <see cref="IsEmpty"/> property is true.
    /// </remarks>
    public static Piece Empty => default;

    /// <summary>
    /// Gets the piece's type, such as Pawn or Knight.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The value is empty; an empty square has no piece type.
    /// </exception>
    public PieceType Type
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("An empty piece has no type");
            }

            return (PieceType)(value & 0b0111);
        }
    }

    /// <summary>
    /// Gets the piece's color: White or Black.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The value is empty; an empty square has no piece color.
    /// </exception>
    public PieceColor Color
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("An empty piece has no color");
            }

            return (PieceColor)(value >> 3);
        }
    }

    /// <summary>
    /// Checks whether another piece has the same color and type, or both are empty.
    /// </summary>
    /// <param name="other">The piece to compare with this one.</param>
    /// <returns>
    /// <see langword="true"/> if both values are empty or have the same color and type;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Piece other)
    {
        return value == other.value;
    }

    /// <summary>
    /// Checks whether an object is a piece equal to this one.
    /// </summary>
    /// <param name="obj">The object to compare, which may be null.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="Piece"/>
    /// equal to this value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Piece other && Equals(other);
    }

    /// <summary>
    /// Checks whether two pieces have the same color and type, or both are empty.
    /// </summary>
    /// <param name="left">The first piece value.</param>
    /// <param name="right">The second piece value.</param>
    /// <returns>
    /// <see langword="true"/> if both values are empty or have the same color and type;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Piece left, Piece right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks whether two piece values are unequal.
    /// </summary>
    /// <param name="left">The first piece value.</param>
    /// <param name="right">The second piece value.</param>
    /// <returns>
    /// <see langword="true"/> if the values differ in emptiness, color, or type;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Piece left, Piece right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns a hash code for use in hash-based collections.
    /// </summary>
    /// <returns>A hash code for this value. Equal pieces have equal hash codes.</returns>
    public override int GetHashCode()
    {
        return value;
    }
}
