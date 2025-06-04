using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AlmadaKing;

/// <summary>
/// Represents a state of Super Tic-Tac-Toe game.
/// </summary>
public class GameState()
{
    const ulong hasInfo = 0b111_111_111;
    readonly ulong[] noughtsBitboards = new ulong[9];
    readonly ulong[] crossesBitboards = new ulong[9];
    int x, y = -1;
    float avaliation = 0;
    ulong end = 0;
    Player player = Player.X;
    readonly int[] fillState = new int[9];
    readonly int[] resultState = new int[9];
    readonly int[] microScores = new int[8 * 9];
    readonly int[] macroScores = new int[8];

    /// <summary>
    /// Do a movement. Do a invalid move create a
    /// instable state.
    /// </summary>
    public void Do(Move move)
    {
        var board = move.Position / 9;
        var square = move.Position % 9;
        var x = square % 3;
        var y = square / 3;
        var pos = 1u << square;

        if (move.Player == Player.X)
        {
            crossesBitboards[board] |= pos;
            UpMicro(board, x);
            UpMicro(board, 3 + x);
            if (x == y)
                UpMicro(board, 6);
            if (x + y == 2)
                UpMicro(board, 7);
        }
        else
        {
            noughtsBitboards[board] |= pos;
            DownMicro(board, x);
            DownMicro(board, 3 + x);
            if (x == y)
                DownMicro(board, 6);
            if (x + y == 2)
                DownMicro(board, 7);
        }

        fillState[board]++;

        player = player switch
        {
            Player.X => Player.O,
            Player.O => Player.X,
            _ => Player.None
        };

        if (fillState[x + 3 * y] == 9)
        {
            this.x = this.y = -1;
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            var result = resultState[i];
            if (result is not 1 or -1)
                continue;

            this.x = this.y = -1;
            return;
        }

        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Undo a movement. Undo a invalid move create
    /// a instable state.
    /// </summary>
    public void Undo(Move move)
    {
        var board = move.Position / 9;
        var square = move.Position % 9;
        var x = square % 3;
        var y = square / 3;
        var pos = 1u << square;

        if (move.Player == Player.X)
        {
            crossesBitboards[board] &= ~pos;
            RevertUpMicro(board, x);
            RevertUpMicro(board, 3 + x);
            if (x == y)
                RevertUpMicro(board, 6);
            if (x + y == 2)
                RevertUpMicro(board, 7);
        }
        else
        {
            noughtsBitboards[board] &= ~pos;
            RevertDownMicro(board, x);
            RevertDownMicro(board, 3 + x);
            if (x == y)
                RevertDownMicro(board, 6);
            if (x + y == 2)
                RevertDownMicro(board, 7);
        }

        fillState[board]++;

        player = player switch
        {
            Player.X => Player.O,
            Player.O => Player.X,
            _ => Player.None
        };

        this.x = move.LastX;
        this.y = move.LastY;
    }

    /// <summary>
    /// Get a avaliation from this state. 1.0 means
    /// that X player is winning. -1.0 means the O
    /// player is winning. 0 means a tie.
    /// </summary>
    public float Avaliate(int depth = 3)
    {
        return AlphaBetaMiniMax(depth, 
            float.NegativeInfinity, 
            float.PositiveInfinity
        );
    }

    public Move PickBest()
    {
        var moves = GetMoves();
        var bag = new ConcurrentBag<(Move move, float aval)>();
        Parallel.ForEach(moves, move =>
        {
            var state = Copy();
            state.Do(move);
            bag.Add((move, state.Avaliate()));
        });

        return
            player == Player.X ?
            bag.MaxBy(x => x.aval).move :
            bag.MinBy(x => x.aval).move;
    }

    /// <summary>
    /// Create a perfect copy from this state.
    /// </summary>
    public GameState Copy()
    {
        var copy = new GameState {
            avaliation = avaliation,
            end = end,
            x = x,
            y = y,
            player = player
        };
        Array.Copy(
            noughtsBitboards,
            copy.noughtsBitboards,
            noughtsBitboards.Length
        );
        Array.Copy(
            crossesBitboards,
            copy.crossesBitboards,
            crossesBitboards.Length
        );
        Array.Copy(
            macroScores,
            copy.macroScores,
            macroScores.Length
        );
        Array.Copy(
            microScores,
            copy.microScores,
            microScores.Length
        );
        Array.Copy(
            fillState,
            copy.fillState,
            fillState.Length
        );
        Array.Copy(
            resultState,
            copy.resultState,
            resultState.Length
        );
        return copy;
    }

    /// <summary>
    /// Get a enumeration of valid moves.
    /// </summary>
    public IEnumerable<Move> GetMoves()
    {
        if (end == 1)
            yield break;

        if (x != -1 && y != -1)
        {
            int board = 3 * y + x;
            ulong spaces = ~(
                noughtsBitboards[board] |
                crossesBitboards[board])
                & hasInfo;
            
            byte pos = (byte)(9 * board);
            while (spaces > 0)
            {
                if ((spaces & 1) > 0)
                {
                    yield return new Move(
                        player, pos,
                        x, y
                    );
                }
                pos++;
                spaces >>= 1;
            }
            yield break;
        }

        for (int b = 0; b < 9; b++)
        {
            if (fillState[b] == 9)
                continue;
            
            if (resultState[b] != 0)
                continue;

            ulong spaces = ~(
                noughtsBitboards[b] |
                crossesBitboards[b])
                & hasInfo;
            
            byte pos = (byte)(9 * b);
            while (spaces > 0)
            {
                if ((spaces & 1) > 0)
                {
                    yield return new Move(
                        player, pos,
                        x, y
                    );
                }
                pos++;
                spaces >>= 1;
            }
        }
    }

    /// <summary>
    /// Returns a string representing the board.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (x % 3 == 0 && x > 0)
                    sb.Append('│');
                
                var boardIndex = (x / 3) + 3 * (y / 3);
                var boardPos = (x % 3) + 3 * (y % 3);
                var board = crossesBitboards[boardIndex];
                var value = (board >> boardPos) & 1;
                if (value == 1)
                {
                    sb.Append('x');
                    continue;
                }
                
                board = noughtsBitboards[boardIndex];
                value = (board >> boardPos) & 1;
                if (value == 1)
                {
                    sb.Append('o');
                    continue;
                }
                
                sb.Append(' ');
            }
            sb.AppendLine();
            if (y % 3 == 2 && y < 8)
                sb.AppendLine("───┼───┼───");
        }

        return sb.ToString();
    }

    void UpMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]++;
        if (microScores[index] == 2)
            avaliation += 0.025f;
        if (microScores[index] < 3)
            return;
        
        resultState[board] = +1;
        var x = board % 3;
        var y = board / 3;
        UpMacro(x);
        UpMacro(3 + y);
        if (x == y)
            UpMacro(6);
        if (x + y == 2)
            UpMacro(7);
    }

    void RevertUpMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        if (microScores[index] == 2)
            avaliation -= 0.025f;
        microScores[index]--;
        if (microScores[index] != 2)
            return;
        
        resultState[board] = 0;
        var x = board % 3;
        var y = board / 3;
        RevertUpMacro(x);
        RevertUpMacro(3 + y);
        if (x == y)
            RevertUpMacro(6);
        if (x + y == 2)
            RevertUpMacro(7);
    }

    void DownMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]--;
        if (microScores[index] == -2)
            avaliation -= 0.025f;
        if (microScores[index] > -3)
            return;
        
        resultState[board] = -1;
        var x = board % 3;
        var y = board / 3;
        DownMacro(x);
        DownMacro(3 + y);
        if (x == y)
            DownMacro(6);
        if (x + y == 2)
            DownMacro(7);
    }
    
    void RevertDownMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        if (microScores[index] == -2)
            avaliation += 0.025f;
        microScores[index]++;
        if (microScores[index] != -2)
            return;
        
        resultState[board] = 0;
        var x = board % 3;
        var y = board / 3;
        RevertDownMacro(x);
        RevertDownMacro(3 + y);
        if (x == y)
            RevertDownMacro(6);
        if (x + y == 2)
            RevertDownMacro(7);
    }
    
    void UpMacro(int pos)
    {
        RevertAvaliationByMacro(pos);
        macroScores[pos]++;
        ChangeAvaliationByMacro(pos);
        if (macroScores[pos] == 3)
            end = 1;
    }

    void RevertUpMacro(int pos)
    {
        RevertAvaliationByMacro(pos);
        macroScores[pos]--;
        ChangeAvaliationByMacro(pos);
        if (macroScores[pos] == 2)
            end = 0;
    }

    void DownMacro(int pos)
    {
        RevertAvaliationByMacro(pos);
        macroScores[pos]--;
        ChangeAvaliationByMacro(pos);
        if (macroScores[pos] == -3)
            end = 1;
    }

    void RevertDownMacro(int pos)
    {
        RevertAvaliationByMacro(pos);
        macroScores[pos]++;
        ChangeAvaliationByMacro(pos);
        if (macroScores[pos] == -2)
            end = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ChangeAvaliationByMacro(int pos)
        => avaliation += GetEvaluationByMacro(pos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RevertAvaliationByMacro(int pos)
        => avaliation -= GetEvaluationByMacro(pos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float GetEvaluationByMacro(int pos)
    {
        return macroScores[pos] switch
        {
            3 => 1f,
            2 => 0.225f,
            1 => 0.075f,
            -1 => -0.075f,
            -2 => -0.225f,
            -3 => -1f,
            _ => 0f
        };
    }

    float AlphaBetaMiniMax(int depth, float alfa, float beta)
    {
        if (depth == 0 || end == 1)
            return avaliation;
        
        if (player == Player.X)
        {
            float value = float.NegativeInfinity;
            foreach (var move in GetMoves())
            {
                Do(move);
                var newValue = AlphaBetaMiniMax(depth - 1, alfa, beta);
                Undo(move);

                value = float.Max(value, newValue);
                if (value > beta)
                    break;
                alfa = float.Max(alfa, value);
            }
            return value;
        }
        else
        {
            float value = float.PositiveInfinity;
            foreach (var move in GetMoves())
            {
                Do(move);
                var newValue = AlphaBetaMiniMax(depth - 1, alfa, beta);
                Undo(move);

                value = float.Min(value, newValue);
                if (value < alfa)
                    break;
                beta = float.Min(beta, value);
            }
            return value;
        }
    }
}