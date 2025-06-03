using AlmadaKing;

var state = new GameState();

for (int k = 0; k < 81; k++)
{
    Console.WriteLine(state);

    var moves = state.GetMoves().ToArray();
    if (moves.Length == 0)
        return;
    
    Random.Shared.Shuffle(moves);
    var randomMove = moves[0];
    state.Do(randomMove);
}
