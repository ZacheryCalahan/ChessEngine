
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

public static class Bitboard
{
    // Useful bitboards
    public static readonly ulong UniversalSet = ulong.MaxValue;
    public static readonly ulong EmptySet = 0;
    public static readonly ulong FileA = 0x101010101010101;
    public static readonly ulong FileB = 0x202020202020202;
    public static readonly ulong FileG = 0x4040404040404040;
    public static readonly ulong FileH = 0x8080808080808080;
    public static readonly ulong Rank1 = 0x00000000000000FF;
    public static readonly ulong Rank2 = 0x000000000000FF00;
    public static readonly ulong Rank7 = 0x00FF000000000000;
    public static readonly ulong Rank8 = 0xFF00000000000000;

    /* Square centric functions */

    public static ulong SquareToBitboard(int square) => (1ul << square);

    public static bool GetSquare(ulong bb, int square) => ((bb >> square) & 1UL) != 0;

    public static ulong SetSquare(ulong bb, int square) => bb |= (1UL << square);

    public static ulong RemoveSquare(ulong bb, int square) => bb &= ~(1UL << square);

    /* Set manipulation functions */

    public static ulong Intersection(ulong a, ulong b) => a & b;

    public static ulong Union(ulong a, ulong b) => a | b;

    public static ulong Complement(ulong a) => ~a;

    public static ulong XOR(ulong a, ulong b) => a ^ b;

    public static ulong Prune(ulong a, ulong b) => a & ~b;

    public static int BitCount(ulong a) => BitOperations.PopCount(a);

    /* Bit finding/manipulation functions */

    public static ulong LSB(ulong a) => a & (0UL - a);

    public static int LSBToSquare(ulong a) => BitOperations.TrailingZeroCount(a);

    public static ulong PopLSB(ulong a) => a &= (a - 1UL);

    public static ulong MSB(ulong a)
    {
        a |= a >> 32;
        a |= a >> 16;
        a |= a >> 8;
        a |= a >> 4;
        a |= a >> 2;
        a |= a >> 1;
        return (a >> 1) + 1;
    }

    public static int MSBToSquare(ulong a)
    {
        return BitOperations.Log2(a);
    }

    public static ulong PopMSB(ulong a)
    {
        if (a == 0)
            return 0;

        return a & ~(1UL << BitOperations.Log2(a));
    }

    /* Directional shifting functions */

    public static ulong North(ulong a) => a << 8;

    public static ulong South(ulong a) => a >> 8;

    public static ulong East(ulong a) => (a << 1) & ~FileA;

    public static ulong West(ulong a) => (a >> 1) & ~FileH;

    public static ulong NorthEast(ulong a) => (a << 9) & ~FileA;

    public static ulong SouthEast(ulong a) => (a >> 7) & ~FileA;

    public static ulong SouthWest(ulong a) => (a >> 9) & ~FileH;

    public static ulong NorthWest(ulong a) => (a << 7) & ~FileH;
    
    /* Helpful definition functions */

    public static bool IsEmpty(ulong a) => a == 0;

    public static bool IsUniversal(ulong a) => a == ulong.MaxValue;

    public static string ToString(ulong a) 
    {
        StringBuilder sb = new StringBuilder();

        // Print from rank 8 down to rank 1
        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;
                bool isSet = ((a >> square) & 1UL) != 0;

                sb.Append(isSet ? "1 " : ". ");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string ToStringMarker(ulong a, int markSquare)
    {
        StringBuilder sb = new StringBuilder();

        // Print from rank 8 down to rank 1
        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;
                bool isSet = ((a >> square) & 1UL) != 0;
                if (square == markSquare)
                {
                    sb.Append("P ");
                } else
                {
                    sb.Append(isSet ? "1 " : ". ");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

}

