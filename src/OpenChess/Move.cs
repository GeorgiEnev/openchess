namespace OpenChess;

/// <summary>
/// Describes a move by its origin, destination, and optional promotion choice.
/// </summary>
/// <remarks>Creating a move records the choice without applying it to a position.</remarks>
public readonly struct Move
{
    /// <summary>
    /// The square the piece moves from.
    /// </summary>
    public Square From { get; }

    /// <summary>
    /// The square the piece moves to.
    /// </summary>
    public Square To { get; }

    /// <summary>
    /// The piece type chosen for promotion, or null for a move without promotion.
    /// </summary>
    public PieceType? Promotion { get; }

    /// <summary>
    /// Creates a move between distinct squares, optionally specifying a pawn promotion.
    /// </summary>
    /// <param name="from">The origin square.</param>
    /// <param name="to">The destination square.</param>
    /// <param name="promotion">Knight, Bishop, Rook, or Queen for promotion; otherwise null.</param>
    /// <remarks>Construction does not check movement rules, placement, or king safety.</remarks>
    /// <exception cref="ArgumentException">
    /// The squares are equal, or the promotion type is unsupported.
    /// </exception>
    public Move(Square from, Square to, PieceType? promotion = null)
    {
        if (from == to)
        {
            throw new ArgumentException("There can no be mathcing origin and distination.");
        }

        if (promotion is not null)
        {
            if (promotion is not (PieceType.Knight or PieceType.Bishop or PieceType.Rook or PieceType.Queen))
            {
                throw new ArgumentException("Wrong promotion piece type.");
            }
        }

        From = from;
        To = to;
        Promotion = promotion;
    }
}
