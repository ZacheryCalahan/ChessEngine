using System.Diagnostics;

public class Search
{
    const int infinity = 99999999;
    public event Action<Move>? OnSearchComplete;
    Move bestMoveThisIteration; // This is the best move found within a single iteration
    int bestEvalThisIteration;
    volatile bool searchCanceled;
    Move bestMove; // This is the best move found within the search
    int bestEval;
    bool hasFoundOneMove;
    Board board;

    Stopwatch sw;

    public Search(Board board)
    {
        sw = new();
        bestMoveThisIteration = new();
        bestEvalThisIteration = -infinity;
        searchCanceled = false;
        hasFoundOneMove = false;
        this.board = board;
    }

    public void StartSearchDeepening()
    {
        searchCanceled = false;
        hasFoundOneMove = false;

        // Iterative deepening search
        for (int searchDepth = 1; searchDepth < int.MaxValue; searchDepth++)
        {
            // Search through this depth, and set the bestMoveThisIteration
            sw.Restart();
            SearchMoves(searchDepth, -infinity, infinity);
            sw.Stop();

            if (searchCanceled)
            {
                Console.WriteLine("Search complete:");
                Console.WriteLine($"Best eval found: {bestEval}");
                break;
            }

            Console.WriteLine($"Depth {searchDepth} time : {sw.ElapsedMilliseconds}ms");

            // Save this as last iterations best move
            bestMove = bestMoveThisIteration;
            bestEval = bestEvalThisIteration;
            bestMoveThisIteration = new();
            bestEvalThisIteration = -infinity;
            hasFoundOneMove = true;
        }

        // In the case search is canceled before a good move is found
        if (!hasFoundOneMove)
        {
            Console.WriteLine("Best move not found!");
            return;
        }

        // Sanity
        var legalMoves = MoveGenerator.GenerateLegalMoves(board);
        if (!legalMoves.Contains(bestMove))
        {
            Console.WriteLine($"Illegal best move selected: {bestMove}");
            bestMove = legalMoves[0]; // fallback
        }

        OnSearchComplete?.Invoke(bestMove);

        OnSearchComplete?.Invoke(bestMove);
    }

    public void StopSearch()
    {
        searchCanceled = true;
    }

    int SearchMoves(int depth, int alpha, int beta, int depthFromRoot = 0)
    {
        if (searchCanceled)
        {
            return 0;
        }

        if (depth == 0)
        {
            return Evaluation.Evaluate(board);
        }

        // Get all available moves and order them
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board); // Should be possible to use pseudolegal?
        MoveOrder.OrderMoves(board, ref moves);

        // If best move exists already, check that move first.
        if (hasFoundOneMove && depthFromRoot == 0)
        {
            int idx = moves.IndexOf(bestMove);
            if (idx > 0)
            {
                moves.RemoveAt(idx);
                moves.Insert(0, bestMove);
            }
        }

        // Check if in check or stalemate
        if (moves.Count == 0)
        {
            // This is temporary!
            return -10000;
        }
        
        

        // Search moves
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];
            board.MakeMove(moves[i]);

            // Search can be extended here for interesting cases

            int eval = -SearchMoves(depth - 1, -beta, -alpha, depthFromRoot + 1);
            board.UnMakeMove(moves[i]);

            if (searchCanceled)
            {
                // Prevent the return eval of 0 on cancel from setting a bad move
                return 0;
            }

            // Stop searching this path if any move would be too good for the opponent
            if (eval >= beta)
            {
                return beta;
            }

            // Check if this move is the best move
            if (eval > alpha)
            {
                alpha = eval;

                // If we're at the root, say this is the best move.
                if (depthFromRoot == 0)
                {
                    bestMoveThisIteration = moves[i];
                    bestEvalThisIteration = eval;
                    hasFoundOneMove = true;
                }
            }

        }

        return alpha;
    }
}

