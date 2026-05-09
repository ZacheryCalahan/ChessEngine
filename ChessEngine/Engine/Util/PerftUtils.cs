
using System.Diagnostics;

public class PerftUtils
{
    Stopwatch sw;
    public PerftUtils()
    {
        sw = new();
    }

    public void PrintPerftResults(Board board, int depth)
    {
        sw.Restart();
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
        sw.Restart();
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int movesCounted = PerftTest(board, depth - 1);
            Console.WriteLine(move.ToUCIMoveString() + ": " + movesCounted);
            board.UnMakeMove(move);
            totalMoves += movesCounted;
        }
        sw.Stop();
        Console.WriteLine($"Nodes searched: {totalMoves} Time: {sw.ElapsedMilliseconds}ms");
    }
}

