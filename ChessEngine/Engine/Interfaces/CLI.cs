public static class CLI
{
    public static void Start()
    {
        //// Check rays
        //for (int i = 0; i < 64; i++)
        //{
        //    Console.WriteLine($"sq {i}\n{Bitboard.ToStringMarker((MoveGenerator.PawnMoves[0, i] | MoveGenerator.PawnAttacks[0, i]), i)}");
        //    Console.ReadLine();
        //}

        //TestEngine();
        Init();
        


    }

    public static void TestEngine()
    {
        // Self test the engine
        TestSuite.TestAll();
    }

    public static void Init()
    {
        Board board = new Board();
        board.ImportBoard("rnb1k1nr/ppp3pp/8/2b5/8/N2P4/PP1P4/R1BK1q2 w kq - 0 26 "); // Check
        BoardUtils.PrintBoardChar(board);

        PerftUtils perft = new();
        perft.PrintPerftTestDivide(board, 1);
    }
    
    
}

