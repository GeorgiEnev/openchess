using System.Globalization;

namespace OpenChess;

/// <summary>
/// Converts FEN text into positions and piece placement.
/// </summary>
/// <remarks>
/// Checks input syntax and field ranges, but does not establish whether a position
/// is legal for play. Parsed positions own newly created boards.
/// </remarks>
internal static class FenParser
{
    internal static Position Parse(string fen)
    {
        ArgumentNullException.ThrowIfNull(fen);

        string[] tokens = fen.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != 6)
        {
            throw new FormatException("FEN must contain exactly six fields.");
        }

        Board board = ParsePlacement(tokens[0]);

        var sideToMoveValues = tokens[1];

        PieceColor sideToMove = sideToMoveValues switch
        {
            "w" => PieceColor.White,
            "b" => PieceColor.Black,
            _ => throw new FormatException("Side to move must be 'w' or 'b'.")
        };

        CastlingRights castlingRights = ParseCastlingRights(tokens[2]);
        Square? enPassantTarget = ParseEnPassantTarget(tokens[3]);

        if (!int.TryParse(tokens[4], NumberStyles.None, CultureInfo.InvariantCulture, out int halfmoveClock))
        {
            throw new FormatException("Halfmove clock must be a nonnegative integer within the Int32 range.");
        }

        if (!int.TryParse(tokens[5], NumberStyles.None, CultureInfo.InvariantCulture, out int fullmoveNumber) || fullmoveNumber < 1)
        {
            throw new FormatException("Fullmove number must be a positive integer within the Int32 range.");
        }

        return new Position(board, sideToMove, castlingRights, enPassantTarget, halfmoveClock, fullmoveNumber);
    }

    /// <summary>
    /// Builds a board from the piece-placement field of a FEN string.
    /// </summary>
    /// <param name="placement">Eight ranks in order from rank 8 to rank 1, separated by slashes.</param>
    /// <returns>A new board containing the described pieces.</returns>
    /// <remarks>
    /// Each rank must describe eight squares using piece letters and single empty-square counts.
    /// Consecutive counts are rejected. This method does not check whether the placement is legal for play.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="placement"/> is null.</exception>
    /// <exception cref="FormatException">The piece-placement field is malformed.</exception>
    internal static Board ParsePlacement(string placement)
    {
        ArgumentNullException.ThrowIfNull(placement);

        string[] ranks = placement.Split('/');

        if (ranks.Length != 8)
        {
            throw new FormatException("Piece placement must contain eight ranks.");
        }

        Board board = new();

        for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
        {
            int rank = 7 - rankIndex;
            ParseRank(ranks[rankIndex], rank, board);
        }

        return board;
    }

    /// <summary>
    /// Places the pieces described by one FEN rank onto the supplied board.
    /// </summary>
    /// <param name="text">A rank description using piece letters and empty-square counts.</param>
    /// <param name="rank">The zero-based board rank, from 0 through 7.</param>
    /// <param name="board">The board being populated by the placement parser.</param>
    /// <exception cref="FormatException">
    /// The description contains an invalid symbol, consecutive counts, or a total other than eight squares.
    /// </exception>
    private static void ParseRank(string text, int rank, Board board)
    {
        int file = 0;
        bool previousWasDigit = false;

        foreach (char symbol in text)
        {
            if (symbol >= '1' && symbol <= '8')
            {
                if (previousWasDigit)
                {
                    throw new FormatException("Each group of consecutive empty squares must use a single digit from 1 to 8");
                }

                // Subtract '0' to convert the digit to a number (e.g. '4' becomes 4).
                // Digit character codes are consecutive: '4' - '0' is 52 - 48, giving the number 4.
                file += symbol - '0';
                previousWasDigit = true;
            }
            else
            {
                Piece piece = ParsePiece(symbol);

                if (file >= 8)
                {
                    throw new FormatException("A rank cannot contain more than eight squares.");
                }

                board.PlacePiece(new Square(file, rank), piece);
                file++;
                previousWasDigit = false;
            }

            if (file > 8)
            {
                throw new FormatException("A rank cannot contain more than eight squares.");
            }
        }

        if (file != 8)
        {
            throw new FormatException("Each rank must describe exactly eight squares.");
        }
    }

    /// <summary>
    /// Converts a FEN piece letter into a piece with the corresponding type and color.
    /// </summary>
    /// <param name="symbol">One of P, N, B, R, Q, or K for White, or its lowercase form for Black.</param>
    /// <returns>The non-empty piece represented by the letter.</returns>
    /// <exception cref="FormatException"><paramref name="symbol"/> is not a supported piece letter.</exception>
    private static Piece ParsePiece(char symbol)
    {
        PieceType type = symbol switch
        {
            'p' or 'P' => PieceType.Pawn,
            'n' or 'N' => PieceType.Knight,
            'b' or 'B' => PieceType.Bishop,
            'r' or 'R' => PieceType.Rook,
            'q' or 'Q' => PieceType.Queen,
            'k' or 'K' => PieceType.King,
            _ => throw new FormatException("Unknown piece symbol.")
        };

        PieceColor color = char.IsUpper(symbol) ? PieceColor.White : PieceColor.Black;

        return new Piece(color, type);
    }

    /// <summary>
    /// Converts the FEN castling field into a combination of castling-right flags.
    /// </summary>
    /// <param name="text">A dash for no rights, or distinct letters from K, Q, k, and q.</param>
    /// <returns>The combined rights, or <see cref="CastlingRights.None"/> for a dash.</returns>
    /// <exception cref="FormatException">The field contains an unsupported or duplicate letter.</exception>
    private static CastlingRights ParseCastlingRights(string text)
    {
        if (text == "-")
        {
            return CastlingRights.None;
        }

        CastlingRights rights = CastlingRights.None;

        foreach (char symbol in text)
        {
            CastlingRights flag = symbol switch
            {
                'K' => CastlingRights.WhiteKingside,
                'Q' => CastlingRights.WhiteQueenside,
                'k' => CastlingRights.BlackKingside,
                'q' => CastlingRights.BlackQueenside,
                _ => throw new FormatException("Castling rights must be '-' or a combination of K, Q, k, and q.")
            };

            // Reject a castling right that is already present, such as a second 'K' in "KK".
            if ((rights & flag) != CastlingRights.None)
            {
                throw new FormatException("Castling rights cannot contain duplicate letters.");
            }

            rights |= flag;
        }

        return rights;
    }

    /// <summary>
    /// Converts the FEN en passant field into an optional target square.
    /// </summary>
    /// <param name="text">A dash for no target, or a lowercase coordinate on rank 3 or 6.</param>
    /// <returns>The target square, or null for a dash.</returns>
    /// <exception cref="FormatException">The coordinate is malformed or is not on rank 3 or 6.</exception>
    private static Square? ParseEnPassantTarget(string text)
    {
        if (text == "-")
        {
            return null;
        }

        Square square = Square.Parse(text);

        if (square.Rank is not (2 or 5))
        {
            throw new FormatException("En passant target must be on rank 3 or 6.");
        }

        return square;
    }
}
