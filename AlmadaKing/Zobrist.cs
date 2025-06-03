using System;

namespace AlmadaKing;

public class Zobrist
{
    readonly ulong[] stringbits;

    public Zobrist()
    {
        const int players = 2;
        const int squares = 9;
        const int boards = 9;
        stringbits = new ulong[
            players * squares * boards
        ];
        for (int i = 0; i < stringbits.Length; i++)
        {
            var first32 = (ulong)(Random.Shared.Next() << 32);
            var last32 = (uint)Random.Shared.Next();
            stringbits[i] = first32 + last32;
        }
    }
}