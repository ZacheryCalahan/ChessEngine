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
        0,  0,  0,  0,  0,  0,  0,  0,
        80, 80, 80, 80, 80, 80, 80, 80,
        60, 60, 60, 60, 60, 60, 60, 60,
        50, 50, 50, 50, 50, 50, 50, 50,
        40, 40, 40, 40, 40, 40, 40, 40,
        30, 30, 30, 30, 30, 30, 30, 30,
        20, 20, 20, 20, 20, 20, 20, 20,
        10, 10, 10, 10, 10, 10, 10, 10,
        0,  0,  0,  0,  0,  0,  0,  0,
    };

    public static readonly int[] Pawns =
    {   // Encourage pawns to protect king and control center, then to attack if pushed forward.
        0,   0,   0,   0,   0,   0,   0,   0,
        30,  30, 30,  30,  30,  30,  30,  30,
        20,  20, 20,  20,  20,  20,  20,  20,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,  30,  30,   0,   0,   0,
        10,  0,   0,  20,  20,   0,   0,  10,
        40, 40,  40,   0,   0,  40,  40,  40,
        0,   0,   0,   0,   0,   0,   0,   0,
    };

    public static readonly int[] Rooks =
    {   // Not too sure here, so prioritize keeping king safe?
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0, 30, 10, 20,  0,  0,
    };

    public static readonly int[] Queens =
    {   //
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
        0,  0,  0,  0,  0,  0,  0,  0,
    };

    public static readonly int[] Knights =
    {   // Encourage center positions
        -10, -10, -10, -10, -10, -10, -10, -10,
        -10,  -5,  -5,  -5,  -5,  -5,  -5, -10, 
        -10,  -5,   0,   0,   0,   0,  -5, -10,
        -10,  -5,   0,   20, 20,   0,  -5, -10,
        -10,  -5,   0,   20, 20,   0,  -5, -10,
        -10,  -5,   0,   0,   0,   0,  -5, -10,
        -10,  -5,  -5,  -5,  -5,  -5,  -5, -10,
        -10, -10, -10, -10, -10, -10, -10, -10,
    };

    public static readonly int[] Bishops =
    {   // Encourage center positions
        -10, -10, -10, -10, -10, -10, -10, -10,
        -10,  -5,  -5,  -5,  -5,  -5,  -5, -10,
        -10,  -5,   0,   0,   0,   0,  -5, -10,
        -10,  -5,   0,   20, 20,   0,  -5, -10,
        -10,  -5,   5,   20, 20,   5,  -5, -10,
        -10,  -5,  10,  10,  10,  10,  -5, -10,
        -10,  -5,  -5,  -5,  -5,  -5,  -5, -10,
        -10, -10, -10, -10, -10, -10, -10, -10,
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

    static int ReverseIndex(int index)
    {
        // Reverse the ranks, but not the files.
        int file = BoardUtils.FileIndex(index);
        int rank = BoardUtils.RankIndex(index);

        rank = 7 - rank;
        return rank * 8 + file;
    }
}
