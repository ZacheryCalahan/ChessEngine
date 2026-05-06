
public static class TestSuite
{
    public static bool TestAll()
    {
        if (!TestMakeUnmake())
        {
            Console.WriteLine("FAILED: Make/Unmake Move");
            return false;
        }

        if (!TestMSBLSB())
        {
            Console.WriteLine("FAILED: MSB/LSB Move");
            return false;
        }

        if (!PiecePerftTests())
        {
            Console.WriteLine("FAILED: Perft Tests");
            return false;
        }

        Console.WriteLine("All tests passed!");
        return true;
    }

    public static bool TestMakeUnmake()
    {
        // Test enpassant
        Board board = new Board();
        board.ImportBoard("7k/8/8/2Pp4/8/8/8/7K w - d6 0 1");
        Board testBoard = new Board(board);

        Move move = new Move(34, 43, Move.EnPassantCapture | Move.PieceCapturedFlag, Piece.BlackPawn);
        board.MakeMove(move);
        board.UnMakeMove(move);

        if (!Board.IsEqual(testBoard, board))
        {
            Console.WriteLine("FAILED: En Passant Move/Unmove Check");
            return false;
        }

        // Test double move
        board = new();
        board.ImportBoard("7k/8/8/2Pp4/8/8/8/7K w - d6 0 1");
        testBoard = new Board(board);
        Move move2 = new Move(63, 55);
        board.MakeMove(move);
        board.MakeMove(move2);
        board.UnMakeMove(move2);
        board.UnMakeMove(move);

        if (!Board.IsEqual(testBoard, board))
        {
            Console.WriteLine("FAILED: Double Move/Unmove Check");
            return false;
        }

        // Test castle
        board = new();
        board.ImportBoard("rnbqkbnr/pp3ppp/2pp4/4p3/4P3/3B1N2/PPPP1PPP/RNBQK2R w KQkq - 0 1");
        testBoard = new Board(board);
        Move castleMove = new Move(4, 6, Move.Castle);
        board.MakeMove(castleMove);
        board.UnMakeMove(castleMove);

        if (!Board.IsEqual(testBoard, board))
        {
            Console.WriteLine("FAILED: Castling Move/Unmove Check");
            return false;
        }



        return true;
    }

    public struct PerftResults
    {
        public string FENString;
        public int Depth;
        public int Positions;

        public PerftResults(string fenString, int depth, int positions)
        {
            FENString = fenString;
            Depth = depth;
            Positions = positions;
        }
    }


    public static bool PiecePerftTests()
    {
        List<PerftResults> tests = new();
        tests.Add(new PerftResults("8/8/8/3k4/5K2/8/8/8 w - - 0 1", 3, 306)); // King tests
        tests.Add(new PerftResults("8/8/8/3k4/4N3/8/8/4K3 w - - 0 1", 2, 82)); // Knight tests
        tests.Add(new PerftResults("8/8/8/3k4/1B6/8/8/4K3 b - - 0 1", 5, 42514)); // Bishop tests
        tests.Add(new PerftResults("8/8/4n3/2k5/4R3/8/8/4K3 w - - 0 1", 3, 3097)); // Rook tests
        tests.Add(new PerftResults("8/8/4n3/2k5/4Q3/8/8/4K3 w - - 0 1", 2, 264)); // Queen tests
        tests.Add(new PerftResults("8/5p2/3p4/4P3/8/8/8/4K2k w - - 0 1", 4, 2005)); // Pawn tests

        // More general perfts
        tests.Add(new PerftResults("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 4, 197281)); // Start pos
        tests.Add(new PerftResults("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 3, 97862)); // Kiwi pete
        tests.Add(new PerftResults("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1 ", 5, 674624)); // Position 3 (chess wiki)

        PerftUtils pf = new();

        foreach (PerftResults test in tests)
        {
            Board board = new Board();
            board.ImportBoard(test.FENString);

            int perftCount = pf.PerftTest(board, test.Depth);
            
            if (perftCount != test.Positions)
            {
                Console.WriteLine($"{test.FENString}: failed. Perft divide:");
                board.ImportBoard(test.FENString);
                pf.PrintPerftTestDivide(board, test.Depth);

                return false;
            }
                

        }
        return true;
    }

    public static bool TestMSBLSB()
    {
        // MSB
        ulong bb = 0x20041000040300;
        if (Bitboard.MSB(bb) != 0x20000000000000)
            return false;
        if (Bitboard.MSBToSquare(bb) != 53)
            return false;
        if (Bitboard.PopMSB(bb) != 0x41000040300)
            return false;

        if (Bitboard.LSB(bb) != 0x100)
            return false;
        if (Bitboard.LSBToSquare(bb) != 8)
            return false;
        if (Bitboard.PopLSB(bb) != 0x20041000040200)
            return false;


        return true;
    }
}

