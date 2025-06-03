namespace AlmadaKing;

/// <summary>
/// Represents a move on the board.
/// </summary>
public readonly struct Move(Player player, byte position)
{
    public readonly byte Position = position;
    public readonly Player Player = player;
};