namespace OpenChess;

/// <summary>
/// Stores chess piece placement and provides square lookup and bitboard queries.
/// </summary>
/// <remarks>
/// A new board is empty. Placement is stored in square entries and in bitboards
/// grouped by type and color. Each bitboard uses <see cref="Square.Index"/> as
/// the bit position; a set bit identifies a matching occupied square.
/// </remarks>
internal sealed class Board
{
    private readonly ulong[] pieceTypeBitboards = new ulong[6];
    private readonly ulong[] pieceColorBitboards = new ulong[2];
    private readonly Piece[] squares = new Piece[64];

    /// <summary>
    /// Returns the piece at the specified square.
    /// </summary>
    /// <param name="square">The square to read.</param>
    /// <returns>The stored piece, or <see cref="Piece.Empty"/> if the square is unoccupied.</returns>
    public Piece GetPiece(Square square)
    {
        return squares[square.Index];
    }

    /// <summary>
    /// Returns a bitboard marking all squares occupied by pieces of the specified type,
    /// regardless of color.
    /// </summary>
    /// <param name="type">The piece type to select.</param>
    /// <returns>A bitboard with a set bit for each matching square, or zero if none match.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="type"/> is not one of the six supported piece types.
    /// </exception>
    public ulong GetPiecesByType(PieceType type)
    {
        if (type < PieceType.Pawn || type > PieceType.King)
        {
            throw new ArgumentOutOfRangeException(
                nameof(type), type, "Type must be a supported chess piece type.");
        }

        return pieceTypeBitboards[(int)type - 1];
    }

    /// <summary>
    /// Returns a bitboard marking all squares occupied by pieces of the specified color,
    /// regardless of type.
    /// </summary>
    /// <param name="color">The piece color to select.</param>
    /// <returns>A bitboard with a set bit for each matching square, or zero if none match.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="color"/> is not White or Black.
    /// </exception>
    public ulong GetPiecesByColor(PieceColor color)
    {
        if (color is not (PieceColor.White or PieceColor.Black))
        {
            throw new ArgumentOutOfRangeException(
                nameof(color), color, "Color must be White or Black.");
        }

        return pieceColorBitboards[(int)color];
    }

    /// <summary>
    /// Returns the squares occupied by pieces matching both the specified type and color.
    /// </summary>
    /// <param name="type">The piece type to select.</param>
    /// <param name="color">The piece color to select.</param>
    /// <returns>A bitboard with a set bit for each matching square, or zero if none match.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="type"/> is not a supported piece type, or
    /// <paramref name="color"/> is not White or Black.
    /// </exception>
    public ulong GetPiecesByTypeAndColor(PieceType type, PieceColor color)
    {
        return GetPiecesByType(type) & GetPiecesByColor(color);
    }

    /// <summary>
    /// The squares occupied by any piece
    /// </summary>
    /// <value>A bitboard with a set bit for each occupied square, or zero when the board is empty.</value>
    public ulong Occupied => pieceColorBitboards[0] | pieceColorBitboards[1];

    /// <summary>
    /// Places a non-empty piece on an unoccupied square.
    /// </summary>
    /// <param name="square">The destination square, which must be empty.</param>
    /// <param name="piece">The piece to place.</param>
    /// <remarks>
    /// Updates the square entry and the corresponding type and color bitboards.
    /// Rejected placement leaves the board unchanged. This operation does not
    /// validate chess movement rules or king safety.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="piece"/> is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="square"/> already contains a piece.
    /// </exception>
    public void PlacePiece(Square square, Piece piece)
    {
        if (piece.IsEmpty)
        {
            throw new ArgumentException("Cannot place an empty piece.", nameof(piece));
        }

        int squareIndex = square.Index;
        Piece currentPiece = GetPiece(square);

        if (!currentPiece.IsEmpty)
        {
            throw new InvalidOperationException("Cannot place a piece on an occupied square.");
        }

        int pieceTypeIndex = (int)piece.Type - 1;
        int colorIndex = (int)piece.Color;
        ulong mask = 1UL << squareIndex;

        pieceTypeBitboards[pieceTypeIndex] |= mask;
        pieceColorBitboards[colorIndex] |= mask;
        squares[squareIndex] = piece;
    }

    /// <summary>
    /// Removes the piece from the specified square, leaving other squares unchanged.
    /// </summary>
    /// <param name="square">The square to clear.</param>
    /// <remarks>
    /// Clears the square entry and its bits in the piece's type and color bitboards.
    /// If the square is already empty, the board is unchanged.
    /// </remarks>
    public void RemovePiece(Square square)
    {
        int squareIndex = square.Index;
        Piece currentPiece = GetPiece(square);

        if (currentPiece.IsEmpty)
        {
            return;
        }

        int pieceTypeIndex = (int)currentPiece.Type - 1;
        int colorIndex = (int)currentPiece.Color;
        ulong mask = 1UL << squareIndex;

        pieceTypeBitboards[pieceTypeIndex] &= ~mask;
        pieceColorBitboards[colorIndex] &= ~mask;
        squares[squareIndex] = Piece.Empty;
    }
}
