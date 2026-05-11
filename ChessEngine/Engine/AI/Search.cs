using System.Diagnostics;

public class Search
{
    Board board;
    Stopwatch sw;
    const int infinity = 99999999;
    const int negativeInfinity = -100000000;

    public event Action<Move>? OnSearchComplete;
    volatile bool searchCanceled;
    bool hasFoundOneMove;

    Move bestMoveThisIteration; // This is the best move found within a single iteration
    int bestEvalThisIteration;
    Move bestMove; // This is the best move found within the search
    int bestEval;
      

    public Search(Board board)
    {
        sw = new();
        bestMoveThisIteration = new();
        bestEvalThisIteration = negativeInfinity;
        searchCanceled = false;
        hasFoundOneMove = false;
        this.board = board;
    }

    public void StartSearchDeepening()
    {
        string fen = board.ExportFen(); // For board comparison (debugging)

        // Initialize
        searchCanceled = false;

        // Iterative deepening search
        int searchDepth = 1;
        for (; searchDepth < int.MaxValue; searchDepth++)
        {
            hasFoundOneMove = false;

            // Search through this depth, and set the bestMoveThisIteration
            sw.Restart();
            SearchMoves(searchDepth, negativeInfinity, infinity);
            sw.Stop();

            if (searchCanceled)
            {
                // Don't throw away useful results! (i think.)
                if (hasFoundOneMove)
                {
                    bestMove = bestMoveThisIteration;
                    bestEval = bestEvalThisIteration;
                }

                Console.WriteLine("Search complete:");
                Console.WriteLine($"Best eval found: {bestEval}");
                break;
            }

            // Save this as last iterations best move
            bestMove = bestMoveThisIteration;
            bestEval = bestEvalThisIteration;
            bestMoveThisIteration = new();
            bestEvalThisIteration = negativeInfinity;
            hasFoundOneMove = true;
            Console.WriteLine($"info score cp {bestEval} depth {searchDepth} time {sw.ElapsedMilliseconds} pv {bestMove.ToUCIMoveString()} "); // Report search findings to UCI
        }

        // In the case search is canceled before a good move is found
        if (bestMove.IsNull)
        {
            Console.WriteLine("Best move not found, giving random move.");
            bestMove = MoveGenerator.GenerateLegalMoves(board)[0]; // Ensure a legal move is returned if best move isn't found.
            return;
        }

        // Ensure board was not malformed
        if (board.ExportFen() != fen)
        {
            Console.WriteLine("ERROR: Search finished with different board than it started with!");
        }

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

        if (depth == 0) // Depth remaining
        {
            // Should do a quiescence search here. (search non-captures)
            return QuiescenceSearch(alpha, beta);
        }

        // Get all available moves and order them
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);
        MoveOrder.OrderMoves(board, ref moves);

        // If best move exists already, check that move first. (swap the moves for now?)
        if (hasFoundOneMove && depthFromRoot == 0)
        {
            int idx = moves.IndexOf(bestMove);
            if (idx > 0)
            {
                Move tmp = moves[0];
                moves[0] = moves[idx];
                moves[idx] = tmp;
            }
        }

        // Check if in check(mate) or stalemate
        if (moves.Count == 0)
        {
            if (board.IsInCheck())
                return -(100000 - depthFromRoot); // Mate score (I do not know why this must be calculated this way.)
            
            return 0;
        }
        
        // Search moves
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            board.MakeMove(moves[i]);
            int eval = -SearchMoves(depth - 1, -beta, -alpha, depthFromRoot + 1);
            board.UnMakeMove(moves[i]);

            if (searchCanceled)
            {
                // Prevent the return eval of 0 on cancel from setting a bad move
                return 0;
            }

            // Pruning
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

    int QuiescenceSearch(int alpha, int beta)
    {
        if (searchCanceled)
        {
            return 0;
        }

        int eval = Evaluation.Evaluate(board); // Captures aren't forced, ensure that bad captures don't overtake good non-captures.

        if (eval >= beta)
        {
            return beta;
        }

        if (eval > alpha)
        {
            alpha = eval;
        }

        List<Move> moves = MoveGenerator.GenerateLegalMoves(board, false, true);
        MoveOrder.OrderMoves(board, ref moves);

        for (int i = 0; i < moves.Count; i++)
        {
            board.MakeMove(moves[i]);
            eval = -QuiescenceSearch(-beta, -alpha);
            board.UnMakeMove(moves[i]);

            if (eval >= beta)
            {
                return beta;
            }

            if (eval > alpha)
            {
                alpha = eval;
            }
        }

        return alpha;
    }
}

