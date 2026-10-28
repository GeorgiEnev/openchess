namespace OpenChess;

/// <summary>
/// Represents piece placement, the side to move, castling rights, en passant state, and move counters.
/// </summary>
/// <remarks>
/// Owns a board and exposes its state for inspection. Construction checks basic field values,
/// not legal playability or whether the position can be reached from the starting position.
/// </remarks>
public class Position
{
    private readonly Board board;

    /// <summary>
    /// The color whose turn it is to move.
    /// </summary>
    public PieceColor SideToMove { get; private set; }

    /// <summary>
    /// The castling rights that remain for both sides.
    /// </summary>
    /// <remarks>A remaining right does not establish that castling is currently legal.</remarks>
    public CastlingRights CastlingRights { get; private set; }

    /// <summary>
    /// The target square for en passant, or null when no target is recorded.
    /// </summary>
    /// <remarks>A recorded target must be on chess rank 3 or 6.</remarks>
    public Square? EnPassantTarget { get; private set; }

    /// <summary>
    /// The number of individual moves since the last pawn move or capture.
    /// </summary>
    /// <value>A nonnegative counter used for move-count draw rules.</value>
    public int HalfmoveClock { get; private set; }

    /// <summary>
    /// The game move number, starting at one and increasing after each Black move.
    /// </summary>
    public int FullmoveNumber { get; private set; }

    /// <summary>
    /// Creates a position from owned placement and explicit state values.
    /// </summary>
    /// <param name="board">The board transferred to this position; it must not be shared with another owner.</param>
    /// <param name="sideToMove">The color to move next: White or Black.</param>
    /// <param name="castlingRights">Any combination of the four supported castling flags, including None.</param>
    /// <param name="enPassantTarget">A square on rank 3 or 6, or null for no target.</param>
    /// <param name="halfmoveClock">The nonnegative count of individual moves since a pawn move or capture.</param>
    /// <param name="fullmoveNumber">The game move number, which must be at least one.</param>
    /// <remarks>
    /// Stores the supplied board reference without copying its placement. The caller must
    /// stop mutating that board independently after construction.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The side is unsupported, castling rights contain unsupported bits, the target has an
    /// unsupported rank, the halfmove clock is negative, or the fullmove number is below one.
    /// </exception>
    internal Position(
        Board board,
        PieceColor sideToMove,
        CastlingRights castlingRights,
        Square? enPassantTarget,
        int halfmoveClock,
        int fullmoveNumber)
    {
        ArgumentNullException.ThrowIfNull(board);

        if (sideToMove is not (PieceColor.White or PieceColor.Black))
        {
            throw new ArgumentOutOfRangeException(
                nameof(sideToMove), sideToMove, "Side to move must be White or Black.");
        }

        // OR combines the four allowed flag bits into the mask 00001111.
        const CastlingRights supportedCastlingRights =
            CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside |
            CastlingRights.BlackKingside | CastlingRights.BlackQueenside;

        // Invert the mask, then AND to detect any unsupported bits in the supplied rights.
        if ((castlingRights & ~supportedCastlingRights) != CastlingRights.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(castlingRights), castlingRights, "Castling rights must contain only supported flags.");
        }

        if (enPassantTarget is Square target && target.Rank is not (2 or 5))
        {
            throw new ArgumentOutOfRangeException(
                nameof(enPassantTarget), enPassantTarget, "En passant target must be on rank 3 or 6.");
        }

        if (halfmoveClock < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(halfmoveClock), halfmoveClock, "Halfmove clock must be zero or greater.");
        }

        if (fullmoveNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fullmoveNumber), fullmoveNumber, "Fullmove number must be one or greater.");
        }

        this.board = board;
        SideToMove = sideToMove;
        CastlingRights = castlingRights;
        EnPassantTarget = enPassantTarget;
        HalfmoveClock = halfmoveClock;
        FullmoveNumber = fullmoveNumber;
    }

    /// <summary>
    /// Returns the piece at the specified square.
    /// </summary>
    /// <param name="square">The square to inspect.</param>
    /// <returns>The piece at the square, or <see cref="Piece.Empty"/> if it is unoccupied.</returns>
    public Piece GetPiece(Square square)
    {
        return board.GetPiece(square);
    }

    /// <summary>
    /// Creates a new position from Forsyth-Edwards Notation (FEN).
    /// <para>
    /// Example FEN: <c>rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1</c>.
    /// </para>
    /// </summary>
    /// <param name="fen">
    /// A complete FEN string with six fields in this order: piece placement, side to move,
    /// castling rights, en passant target, halfmove clock, and fullmove number.
    /// </param>
    /// <returns>A position containing the placement and state described by <paramref name="fen"/>.</returns>
    /// <remarks>
    /// Loads the pieces and state recorded in the FEN into a new position.
    /// Validates the FEN format without checking whether the position is legal for play.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="fen"/> is null.</exception>
    /// <exception cref="FormatException">
    /// The input does not contain exactly six fields, or a field is malformed or outside its accepted range.
    /// </exception>
    public static Position ParseFen(string fen)
    {
        return FenParser.Parse(fen);
    }
}
