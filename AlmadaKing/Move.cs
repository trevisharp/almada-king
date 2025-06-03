namespace AlmadaKing;

/// <summary>
/// Represents a move on the board.
/// </summary>
public readonly struct Move(
    Player player, byte position,
    int lastX, int lastY
    )
{
    public readonly byte Position = position;
    public readonly Player Player = player;
    public readonly int LastX = lastX;
    public readonly int LastY = lastY;

    public override string ToString()
        => $"{Player} plays on {Position}";
}