namespace OpenChess;

/// <summary>
/// Defines coordinate offsets for individual destinations and directions for sliding movement.
/// </summary>
internal static class MovePatterns
{
    /// <summary>
    /// File and rank changes for the eight knight jumps.
    /// </summary>
    internal static readonly (int FileOffset, int RankOffset)[] KnightOffsets =
    [
        (1, 2), (2, 1), (2, -1), (1, -2),
        (-1, -2), (-2, -1), (-2, 1), (-1, 2)
    ];

    /// <summary>
    /// File and rank changes for the eight squares adjacent to a king.
    /// </summary>
    internal static readonly (int FileOffset, int RankOffset)[] KingOffsets =
    [
        (-1, -1), (0, -1), (1, -1),
        (-1, 0), (1, 0),
        (-1, 1), (0, 1), (1, 1)
    ];

    /// <summary>
    /// Horizontal and vertical steps followed repeatedly for rook and queen movement.
    /// </summary>
    internal static readonly (int FileOffset, int RankOffset)[] RookDirections =
    [
        (1, 0), (-1, 0), (0, 1), (0, -1)
    ];

    /// <summary>
    /// Diagonal steps followed repeatedly for bishop and queen movement.
    /// </summary>
    internal static readonly (int FileOffset, int RankOffset)[] BishopDirections =
    [
        (1, 1), (1, -1), (-1, 1), (-1, -1)
    ];

}
