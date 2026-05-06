
using System.Diagnostics;

public class PerftUtils
{
    public void PrintPerftResults(Board board, int depth)
    {
        Stopwatch sw = new();
        sw.Start();
        Console.WriteLine($"Found moves: {PerftTest(board, depth)}");
        Console.WriteLine($"Time searched: {sw.ElapsedMilliseconds}ms.");
        sw.Stop();
    }

    public int PerftTest(Board board, int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);
        int numPos = 0;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            numPos += PerftTest(board, depth - 1);
            board.UnMakeMove(move);   
        }

        return numPos;
    }

    public void PrintPerftTestDivide(Board board, int depth)
    {
        if (depth == 0)
        {
            return;
        }

        int totalMoves = 0;
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int movesCounted = PerftTest(board, depth - 1);
            Console.WriteLine(move.ToString().Substring(0, 4) + ": " + movesCounted);
            board.UnMakeMove(move);
            totalMoves += movesCounted;
        }

        Console.WriteLine("Nodes searched: " + totalMoves);
    }
}

