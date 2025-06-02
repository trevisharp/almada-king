namespace AlmadaKing;

/// <summary>
/// Represents a move on the board.
/// </summary>
public readonly struct Move(Player player, int position)
{
    public readonly int Position = position;
    public readonly Player Player = player;
};