public class Search
{
    public event Action<Move>? OnSearchComplete;
    Move bestMoveThisIteration;
    int bestEvalThisIteration;
    volatile bool searchCanceled;
    Move bestMove;
    bool hasFoundOneMove;
    Board board;

    public Search(Board board)
    {
        bestMoveThisIteration = new();
        bestEvalThisIteration = int.MinValue;
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
            SearchMoves(searchDepth, int.MinValue, int.MaxValue);

            if (searchCanceled)
            {
                Console.WriteLine("Search stopped.");
                break;
            }
            bestMove = bestMoveThisIteration;
            hasFoundOneMove = true;
        }

        // In the case search is canceled before a good move is found, just return any move.
        if (!hasFoundOneMove)
        {
            Console.WriteLine("Best move not found!");
            return;
        }

        OnSearchComplete?.Invoke(bestMoveThisIteration);
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
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);
        
        // Check if in check or stalemate
        if (moves.Count == 0)
        {
            return -10000;
        }
        
        MoveOrder.OrderMoves(board, ref moves);

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

