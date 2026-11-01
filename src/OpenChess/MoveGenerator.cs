namespace OpenChess;

/// <summary>
/// Calculates candidate moves from placement and position state without applying them or checking king safety.
/// </summary>
internal static class MoveGenerator
{
    /// <summary>
    /// Collects candidate moves for the selected piece using its movement pattern and the current occupancy.
    /// </summary>
    /// <param name="position">The position to inspect.</param>
    /// <param name="from">The selected origin square.</param>
    /// <returns>
    /// The candidate moves, excluding castling, or an empty collection if the origin is empty
    /// or its piece does not belong to the side to move.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="position"/> is null.</exception>
    internal static IReadOnlyList<Move> Generate(Position position, Square from)
    {
        ArgumentNullException.ThrowIfNull(position);

        var piece = position.GetPiece(from);

        if (piece.IsEmpty)
        {
            return [];
        }
        else if (piece.Color != position.SideToMove)
        {
            return [];
        }

        List<Move> moves = new List<Move>();

        switch (piece.Type)
        {
            case PieceType.Pawn:
                GeneratePawnMoves(position, from, piece, moves);
                break;
            case PieceType.Knight:
                GenerateOffsetMoves(position, from, piece, MovePatterns.KnightOffsets, moves);
                break;
            case PieceType.King:
                GenerateOffsetMoves(position, from, piece, MovePatterns.KingOffsets, moves);
                break;
            case PieceType.Bishop:
                GenerateSlidingMoves(position, from, piece, MovePatterns.BishopDirections, moves);
                break;
            case PieceType.Rook:
                GenerateSlidingMoves(position, from, piece, MovePatterns.RookDirections, moves);
                break;
            case PieceType.Queen:
                GenerateSlidingMoves(position, from, piece, MovePatterns.RookDirections, moves);
                GenerateSlidingMoves(position, from, piece, MovePatterns.BishopDirections, moves);
                break;
        }
        return moves;
    }

    /// <summary>
    /// Checks calculated coordinates before they are used to construct a square.
    /// </summary>
    /// <param name="file">The calculated file.</param>
    /// <param name="rank">The calculated rank.</param>
    /// <returns>True when both coordinates are between zero and seven.</returns>
    private static bool IsInsideBoard(int file, int rank)
    {
        return file is >= 0 and <= 7 &&
               rank is >= 0 and <= 7;
    }

    /// <summary>
    /// Adds knight or king candidates by inspecting each fixed-offset destination independently.
    /// </summary>
    /// <param name="position">The position supplying destination occupants.</param>
    /// <param name="from">The moving piece's origin.</param>
    /// <param name="piece">The moving piece, used to distinguish friendly and opposing occupants.</param>
    /// <param name="offsets">File and rank changes relative to the origin.</param>
    /// <param name="moves">The collection receiving empty-square moves and opposing non-king captures.</param>
    private static void GenerateOffsetMoves(
        Position position,
        Square from,
        Piece piece,
        (int FileOffset, int RankOffset)[] offsets,
        List<Move> moves)
    {
        foreach (var (FileOffset, RankOffset) in offsets)
        {
            int file = from.File + FileOffset;
            int rank = from.Rank + RankOffset;

            if (!IsInsideBoard(file, rank))
            {
                continue;
            }

            Square destination = new Square(file, rank);
            var occupant = position.GetPiece(destination);

            if (occupant.IsEmpty)
            {
                moves.Add(new Move(from, destination));
            }
            else if (occupant.Color != piece.Color && occupant.Type != PieceType.King)
            {
                moves.Add(new Move(from, destination));
            }
        }
    }

    /// <summary>
    /// Adds candidates along each ray, stopping at the first occupied square or the board edge.
    /// </summary>
    /// <param name="position">The position supplying square occupants.</param>
    /// <param name="from">The sliding piece's origin.</param>
    /// <param name="piece">The moving piece, used to distinguish friendly and opposing occupants.</param>
    /// <param name="directions">Nonzero file and rank steps followed repeatedly from the origin.</param>
    /// <param name="moves">
    /// The collection receiving candidates, including the first opposing occupant when it is not a king.
    /// </param>
    private static void GenerateSlidingMoves(
        Position position,
        Square from,
        Piece piece,
        (int FileOffset, int RankOffset)[] directions,
        List<Move> moves)
    {
        foreach (var (FileOffset, RankOffset) in directions)
        {
            int file = from.File + FileOffset;
            int rank = from.Rank + RankOffset;

            while (IsInsideBoard(file, rank))
            {
                Square destination = new Square(file, rank);
                var occupant = position.GetPiece(destination);

                if (occupant.IsEmpty)
                {
                    moves.Add(new Move(from, destination));
                }
                else
                {
                    if (occupant.Color != piece.Color && occupant.Type != PieceType.King)
                    {
                        moves.Add(new Move(from, destination));
                    }
                    break;
                }

                file += FileOffset;
                rank += RankOffset;
            }

        }
    }

    /// <summary>
    /// Adds pawn advances, captures, promotions, and en passant candidates without checking king safety.
    /// </summary>
    /// <param name="position">The position supplying placement and the recorded en passant target.</param>
    /// <param name="from">The pawn's origin.</param>
    /// <param name="piece">The moving pawn.</param>
    /// <param name="moves">The collection receiving the candidates.</param>
    private static void GeneratePawnMoves(
        Position position,
        Square from,
        Piece piece,
        List<Move> moves)
    {
        int direction = piece.Color == PieceColor.White ? 1 : -1;
        int startingRank = piece.Color == PieceColor.White ? 1 : 6;
        int promotionRank = piece.Color == PieceColor.White ? 7 : 0;
        int enPassantCaptureRank = piece.Color == PieceColor.White ? 4 : 3;
        int nextRank = from.Rank + direction;

        if (!IsInsideBoard(from.File, nextRank))
        {
            return;
        }

        Square forward = new(from.File, nextRank);
        if (position.GetPiece(forward).IsEmpty)
        {
            AddPawnMove(from, forward, promotionRank, moves);

            // A double advance also requires the intermediate square to be empty.
            if (from.Rank == startingRank)
            {
                Square doubleAdvance = new(from.File, from.Rank + 2 * direction);
                if (position.GetPiece(doubleAdvance).IsEmpty)
                {
                    moves.Add(new Move(from, doubleAdvance));
                }
            }
        }

        for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
        {
            int file = from.File + fileOffset;
            if (!IsInsideBoard(file, nextRank))
            {
                continue;
            }

            Square destination = new(file, nextRank);
            Piece occupant = position.GetPiece(destination);

            if (!occupant.IsEmpty)
            {
                if (occupant.Color != piece.Color && occupant.Type != PieceType.King)
                {
                    AddPawnMove(from, destination, promotionRank, moves);
                }
            }
            else if (from.Rank == enPassantCaptureRank &&
                     position.EnPassantTarget is Square target && target == destination)
            {
                // The captured pawn stands beside the origin, behind the empty target.
                Square capturedSquare = new(destination.File, from.Rank);
                Piece capturedPiece = position.GetPiece(capturedSquare);
                if (!capturedPiece.IsEmpty && capturedPiece.Color != piece.Color &&
                    capturedPiece.Type == PieceType.Pawn)
                {
                    moves.Add(new Move(from, destination));
                }
            }
        }
    }

    /// <summary>
    /// Adds four promotion choices on the final rank, or one ordinary pawn move elsewhere.
    /// </summary>
    /// <param name="from">The pawn's origin.</param>
    /// <param name="to">The destination already checked by pawn generation.</param>
    /// <param name="promotionRank">The final numeric rank for the pawn's color.</param>
    /// <param name="moves">The collection receiving the candidates.</param>
    private static void AddPawnMove(Square from, Square to, int promotionRank, List<Move> moves)
    {
        if (to.Rank == promotionRank)
        {
            moves.Add(new Move(from, to, PieceType.Queen));
            moves.Add(new Move(from, to, PieceType.Rook));
            moves.Add(new Move(from, to, PieceType.Bishop));
            moves.Add(new Move(from, to, PieceType.Knight));
        }
        else
        {
            moves.Add(new Move(from, to));
        }
    }
}
