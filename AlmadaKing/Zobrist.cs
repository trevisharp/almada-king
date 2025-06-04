using System;
using System.Runtime.CompilerServices;

namespace AlmadaKing;

/// <summary>
/// A generator for Zobrist hashes.
/// </summary>
public class Zobrist
{
    public static readonly Zobrist Shared = new();
    const int players = 2;
    const int squares = 9;
    const int boards = 9;
    readonly ulong[] stringbits;

    public Zobrist()
    {
        stringbits = new ulong[
            players * boards * squares
        ];
        for (int i = 0; i < stringbits.Length; i++)
        {
            var first32 = (ulong)(Random.Shared.Next() << 32);
            var last32 = (uint)Random.Shared.Next();
            stringbits[i] = first32 + last32;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ulong Get(int player, int board, int square)
    {
        var index = 
            player * boards * squares +
            board * squares +
            square;
        return stringbits[index];
    }
}