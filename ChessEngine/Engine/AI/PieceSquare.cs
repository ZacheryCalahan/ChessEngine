using System;

public static class PieceSquare
{

    public static int GetScore(int[] table, int square, bool isWhite)
    {
        if (isWhite)
        {
            square = ReverseIndex(square);
        }

        return table[square];
    }

    // Tables
    static int[,] pieceTable = new int[Piece.PieceIndexCount, 64];

    public static readonly int[] PawnsEnd =
    {   // Encourage pawns to promote
        0,  0,   0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        25, 25, 25, 25, 25, 25, 25, 25,
        20, 20, 20, 20, 20, 20, 20, 20,
        15, 15, 15, 15, 15, 15, 15, 15,
        9,   9,  9,  9,  9,  9,  9,  9,
        6,   6,  6,  6,  6,  6,  6,  6,
        3,   3,  3,  3,  3,  3,  3,  3,
        0,   0,  0,  0,  0,  0,  0,  0,
    };

    public static readonly int[] Pawns =
    {   // Encourage pawns to protect king and control center, then to attack if pushed forward.
          0,   0,   0,   0,   0,   0,   0,   0,
         30,  30,  30,  30,  30,  30,  30,  30,
         10,  10,  10,  10,  10,  10,  10,  10,
          0,   0,   0,   0,   0,   0,   0,   0,
        -20, -20, -20,  30,  30, -20, -20, -20,
         10,   0,   0,  20,  20,   0,   0,  70,
         40,  40,  40,  -40, -40,  70,  70,  35, // King defense and escape square
          0,   0,   0,   0,   0,   0,   0,   0,
    };

    public static readonly int[] Rooks =
    {   // Keep king safe, and decentivise weak squares on the edge.
          0,  0,  0,  0,  0,  0,  0,  0,
          5, 10, 10, 10, 10, 10, 10,  5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
          0,  0,  0,  5,  5,  0,  0,  0
    };

    public static readonly int[] Queens =
    {   // Does not matter much, but keep queen away from edge files.
        -20, -10, -10,  -5,  -5, -10, -10, -20,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,   5,   5,   5,   5,   0, -10,
         -5,   0,   5,   5,   5,   5,   0,  -5,
          0,   0,   5,   5,   5,   5,   0,  -5,
        -10,   5,   5,   5,   5,   5,   0, -10,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -20, -10, -10,  -5,  -5, -10, -10, -20
    };

    public static readonly int[] Knights =
    {   // Encourage center positions
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,  -5,  -5,  -5,  -5,  -5, -40,
        -40,   0,  10,  15,  15,  10,  -5, -30,
        -30,   5,  15,  20,  20,  15,  -5, -30,
        -30,   0,  15,  20,  20,  15,  -5, -30,
        -30,   5,  10,  15,  15,  10,   5, -30,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50,
    };

    public static readonly int[] Bishops =
    {   // Encourage center positions
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,   5,  10,  10,   5,   0, -10,
        -10,   5,   5,  10,  10,   5,   5, -10,
        -10,   0,  10,  10,  10,  10,   0, -10,
        -10,  10,  10,  10,  10,  10,  10, -10,
        -10,   5,   0,   0,   0,   0,   5, -10,
        -20, -10, -10, -10, -10, -10, -10, -20,
    };

    public static readonly int[] Kings =
    {   // Encourage safe play (and castling!)
        -80, -70, -70, -70, -70, -70, -70, -80,
        -60, -60, -60, -60, -60, -60, -60, -60,
        -40, -50, -50, -60, -60, -50, -50, -40,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        20,   20,  -5,  -5,  -5,  -5,  20,  20,
        20,   30,  10,   0,   0,  10,  30,  20
    };

    public static readonly int[] Neutral =
    {   // Neutral board
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    static int ReverseIndex(int index)
    {
        // Reverse the ranks, but not the files.
        int file = BoardUtils.FileIndex(index);
        int rank = BoardUtils.RankIndex(index);

        rank = 7 - rank;
        return rank * 8 + file;
    }
}
