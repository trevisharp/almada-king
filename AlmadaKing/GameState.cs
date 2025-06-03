using System.Text;
using System.Collections.Generic;

namespace AlmadaKing;

public struct GameState()
{
    const ulong hasInfo = 0b111_111_111;
    ulong[] noughtsBitboards = new ulong[9];
    ulong[] crossesBitboards = new ulong[9];
    int x, y = -1;
    Player player = Player.X;
    int[] fillState = new int[9];
    int[] resultState = new int[9];
    int[] microScores = new int[8 * 9];
    int[] macroScores = new int[8];

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

    readonly void UpMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]++;
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

    readonly void DownMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]--;
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
    
    readonly void UpMacro(int pos)
        => macroScores[pos]++;

    readonly void DownMacro(int pos)
        => macroScores[pos]--;

    public void Undo(Move move)
    {

    }

    public float Avaliate()
    {
        throw new System.NotImplementedException();
    }

    public Move PickBest()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<Move> GetMoves()
    {
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
}