namespace AlmadaKing;

public struct GameState()
{
    ulong noughtsBitboard = 0;
    ulong crossesBitboard = 0;
    int playInfo = 10;
    int[] microScores = new int[8 * 9];
    int[] macroScores = new int[8];
}