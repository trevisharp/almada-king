using AlmadaKing;

var state = new GameState();

Stack<Move> moves = [];

for (int k = 0; k < 81; k++)
{
    Console.WriteLine(state);
    Console.WriteLine(state.Avaliate());

    var nextmoves = state.GetMoves().ToArray();
    if (nextmoves.Length == 0)
        break;
    
    Random.Shared.Shuffle(nextmoves);
    var randomMove = nextmoves[0];
    moves.Push(randomMove);
    state.Do(randomMove);
}

// while (moves.Count > 0)
// {
//     var move = moves.Pop();
//     state.Undo(move);
//     Console.WriteLine(state);
// }