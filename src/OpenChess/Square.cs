namespace OpenChess;

/// <summary>
/// Represents a board square using zero-based file and rank coordinates.
/// </summary>
/// <remarks>
/// Indices run from 0 for a1 to 63 for h8, increasing across each rank.
/// <c>default(Square)</c> represents a1.
/// </remarks>
public readonly struct Square : IEquatable<Square>
{
    private readonly byte index;

    /// <summary>
    /// Creates a square from zero-based file and rank coordinates.
    /// </summary>
    /// <param name="file">The file, from 0 for a through 7 for h.</param>
    /// <param name="rank">The rank, from 0 for rank 1 through 7 for rank 8.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Either coordinate is outside the range 0 through 7.
    /// </exception>
    public Square(int file, int rank)
    {
        if (rank < 0 || rank > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(rank));
        }
        if (file < 0 || file > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(file));
        }

        index = (byte)(rank * 8 + file);
    }

    /// <summary>
    /// Gets the index, calculated as rank * 8 + file, from 0 through 63.
    /// </summary>
    /// <remarks>
    /// The index is the square's bit position in a bitboard.
    /// </remarks>
    public int Index => index;

    /// <summary>
    /// Gets the zero-based file, from 0 for a through 7 for h.
    /// </summary>
    public int File => index % 8;

    /// <summary>
    /// Gets the zero-based rank, from 0 for rank 1 through 7 for rank 8.
    /// </summary>
    public int Rank => index / 8;

    /// <summary>
    /// Checks whether another square identifies the same board location.
    /// </summary>
    /// <param name="other">The square to compare.</param>
    /// <returns>True if both squares have the same index; otherwise, false.</returns>
    public bool Equals(Square other)
    {
        return index == other.index;
    }

    /// <summary>
    /// Checks whether an object is a square identifying the same board location.
    /// </summary>
    /// <param name="obj">The object to compare, which may be null.</param>
    /// <returns>True if the object is an equal square; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Square other && Equals(other);
    }

    /// <summary>
    /// Checks whether two squares identify the same board location.
    /// </summary>
    /// <param name="left">The first square.</param>
    /// <param name="right">The second square.</param>
    /// <returns>True if the squares have the same index; otherwise, false.</returns>
    public static bool operator ==(Square left, Square right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks whether two squares identify different board locations.
    /// </summary>
    /// <param name="left">The first square.</param>
    /// <param name="right">The second square.</param>
    /// <returns>True if the squares have different indices; otherwise, false.</returns>
    public static bool operator !=(Square left, Square right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns a hash code for use in hash-based collections.
    /// </summary>
    /// <returns>A hash code consistent with square equality.</returns>
    public override int GetHashCode()
    {
        return index;
    }

    /// <summary>
    /// Returns the square's lowercase algebraic coordinate, such as e4.
    /// </summary>
    /// <returns>A two-character coordinate from a1 through h8.</returns>
    public override string ToString()
    {
        char file = (char)('a' + File);
        char rank = (char)('1' + Rank);

        return new string([file, rank]);
    }

    /// <summary>
    /// Attempts to convert a lowercase algebraic coordinate to a square.
    /// </summary>
    /// <param name="text">
    /// Exactly one file character from a through h followed by a rank digit from 1 through 8.
    /// Uppercase characters and surrounding whitespace are not accepted.
    /// </param>
    /// <param name="square">
    /// The parsed square on success; otherwise, <c>default(Square)</c>, representing a1.
    /// Use this output as a parsed result only when the method returns true.
    /// </param>
    /// <returns>True if the text is valid; false for null or invalid text.</returns>
    public static bool TryParse(string? text, out Square square)
    {
        square = default;

        if (text is null || text.Length != 2)
        {
            return false;
        }

        char fileCharacter = text[0];
        char rankCharacter = text[1];

        if (fileCharacter < 'a' || fileCharacter > 'h')
        {
            return false;
        }

        if (rankCharacter < '1' || rankCharacter > '8')
        {
            return false;
        }

        int file = fileCharacter - 'a';
        int rank = rankCharacter - '1';

        square = new Square(file, rank);

        return true;
    }

    /// <summary>
    /// Converts a lowercase algebraic coordinate to a square.
    /// </summary>
    /// <param name="text">
    /// Exactly one file character from a through h followed by a rank digit from 1 through 8.
    /// Uppercase characters and surrounding whitespace are not accepted.
    /// </param>
    /// <returns>The square identified by the coordinate.</returns>
    /// <exception cref="ArgumentNullException">The text is null.</exception>
    /// <exception cref="FormatException">The text is not a valid coordinate.</exception>
    public static Square Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!Square.TryParse(text, out Square square))
        {
            throw new FormatException("Expected a square from a1 to h8.");
        }

        return square;
    }
}
