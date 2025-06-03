using System.Collections;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace AlmadaKing;

public struct GameState()
{
    const ulong hasInfo = 0b111_111_111;
    ulong[] noughtsBitboards = new ulong[9];
    ulong[] crossesBitboards = new ulong[9];
    int x, y = -1;
    Player player = Player.X;
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
            crossesBitboards[board] &= pos;
            UpMicro(board, x);
            UpMicro(board, 3 + x);
            if (x == y)
                UpMicro(board, 6);
            if (x + y == 2)
                UpMicro(board, 7);
        }
        else
        {
            noughtsBitboards[board] &= pos;
            DownMicro(board, x);
            DownMicro(board, 3 + x);
            if (x == y)
                DownMicro(board, 6);
            if (x + y == 2)
                DownMicro(board, 7);
        }

        player = player switch
        {
            Player.X => Player.O,
            Player.O => Player.X,
            _ => Player.None
        };

        var targetScore = macroScores[board];
        if (targetScore is 3 or -3)
            this.x = this.y = -1;
        else
        {
            this.x = x;
            this.y = y;
        }
    }

    private void UpMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]++;
        if (microScores[index] < 3)
            return;
        
        var x = board % 3;
        var y = board / 3;
        macroScores[x]++;
        macroScores[3 + y]++;
        if (x == y)
            macroScores[6]++;
        if (x + y == 2)
            macroScores[7]++;        
    }

    private void DownMicro(int board, int pos)
    {
        var index = 8 * board + pos;
        microScores[index]--;
        if (microScores[index] > -3)
            return;
        
        var x = board % 3;
        var y = board / 3;
        macroScores[x]--;
        macroScores[3 + y]--;
        if (x == y)
            macroScores[6]--;
        if (x + y == 2)
            macroScores[7]--;        
    }
    
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
                    yield return new Move(
                        player, pos,
                        x, y
                    );
            }
            yield break;
        }

        for (int b = 0; b < 9; b++)
        {
            ulong spaces = ~(
                noughtsBitboards[b] |
                crossesBitboards[b])
                & hasInfo;
            
            byte pos = (byte)(9 * b);
            while (spaces > 0)
            {
                if ((spaces & 1) > 0)
                    yield return new Move(
                        player, pos,
                        x, y
                    );
            }

        }
    }
}