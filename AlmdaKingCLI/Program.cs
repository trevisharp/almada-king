using AlmadaKing;

var state = new GameState();

Console.WriteLine(state);
Console.WriteLine($"Moves: {state.GetMoves().Count()}");

for (int k = 0; k < 10; k++)
{
    var randomMove = state.GetMoves()
        .Skip(Random.Shared.Next(80))
        .FirstOrDefault();
    state.Do(randomMove);

    Console.WriteLine(state);
    Console.WriteLine($"Moves: {state.GetMoves().Count()}");
}
