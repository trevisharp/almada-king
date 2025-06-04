using AlmadaKing;

var state = new GameState();

for (int k = 0; k < 81; k++)
{
    var nextmoves = state.GetMoves().ToArray();
    if (nextmoves.Length == 0)
        break;
    
    Random.Shared.Shuffle(nextmoves);
    var randomMove = nextmoves[0];
    state.Do(randomMove);

    var aiMove = state.PickBest();
    state.Do(aiMove);
    
    Console.WriteLine(state);
    Console.WriteLine(state.Avaliate());
    Console.ReadKey(true);
}