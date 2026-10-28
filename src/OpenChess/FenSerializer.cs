using System.Globalization;
using System.Text;

namespace OpenChess;

/// <summary>
/// Converts a position's placement and state into Forsyth-Edwards Notation (FEN).
/// </summary>
internal static class FenSerializer
{
    /// <summary>
    /// Writes all six FEN fields, separated by single spaces, without modifying the position.
    /// </summary>
    /// <param name="position">The position to serialize.</param>
    /// <returns>A complete FEN string with castling rights in KQkq order and invariant move counters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="position"/> is null.</exception>
    internal static string Serialize(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);

        string placement = SerializePlacement(position);

        string sideToMove = position.SideToMove == PieceColor.White ? "w" : "b";

        string castlingRights = SerializeCastlingRights(position.CastlingRights);

        string enPassantTarget = position.EnPassantTarget is Square target ? target.ToString() : "-";

        string halfmoveClock = position.HalfmoveClock.ToString(CultureInfo.InvariantCulture);

        string fullmoveNumber = position.FullmoveNumber.ToString(CultureInfo.InvariantCulture);

        return string.Join(
            " ",
            placement,
            sideToMove,
            castlingRights,
            enPassantTarget,
            halfmoveClock,
            fullmoveNumber);
    }

    /// <summary>
    /// Writes ranks from 8 to 1 and files from a to h, replacing each run of empty squares
    /// with its count and separating ranks with slashes.
    /// </summary>
    /// <param name="position">The position whose pieces are read.</param>
    /// <returns>The piece placement field of a FEN string.</returns>
    private static string SerializePlacement(Position position)
    {
        StringBuilder sb = new();

        for (int rank = 7; rank >= 0; rank--)
        {
            int emptyCount = 0;
            for (int file = 0; file < 8; file++)
            {
                Piece piece = position.GetPiece(new Square(file, rank));

                if (piece.IsEmpty)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        sb.Append((char)('0' + emptyCount));
                        emptyCount = 0;
                    }

                    sb.Append(SerializePiece(piece));
                }
            }

            if (emptyCount > 0)
            {
                sb.Append((char)('0' + emptyCount));
            }

            if (rank > 0)
            {
                sb.Append('/');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts an occupied square's piece into its FEN letter, uppercase for White and lowercase for Black.
    /// </summary>
    /// <param name="piece">A nonempty piece.</param>
    /// <returns>The letter for the piece type and color; knights use N or n.</returns>
    private static char SerializePiece(Piece piece)
    {
        char pieceType = piece.Type switch
        {
            PieceType.Pawn => 'p',
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b',
            PieceType.Rook => 'r',
            PieceType.Queen => 'q',
            PieceType.King => 'k',
            _ => throw new ArgumentOutOfRangeException(nameof(piece), "The piece type is unsupported.")
        };

        if (piece.Color == PieceColor.White)
        {
            pieceType = char.ToUpperInvariant(pieceType);
        }

        return pieceType;
    }

    /// <summary>
    /// Writes the remaining castling flags in KQkq order.
    /// </summary>
    /// <param name="rights">The position's supported castling flags.</param>
    /// <returns>The letters for the present flags, or a hyphen when no rights remain.</returns>
    private static string SerializeCastlingRights(CastlingRights rights)
    {
        StringBuilder builder = new();

        if ((rights & CastlingRights.WhiteKingside) != CastlingRights.None)
        {
            builder.Append('K');
        }

        if ((rights & CastlingRights.WhiteQueenside) != CastlingRights.None)
        {
            builder.Append('Q');
        }

        if ((rights & CastlingRights.BlackKingside) != CastlingRights.None)
        {
            builder.Append('k');
        }

        if ((rights & CastlingRights.BlackQueenside) != CastlingRights.None)
        {
            builder.Append('q');
        }

        return builder.Length == 0 ? "-" : builder.ToString();
    }
}
