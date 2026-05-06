public class Search
{

    public Search()
    {

    }

    public Move StartSearch(Board board)
    {
        int bestEval = int.MinValue;
        Move bestMove = new();

        List<Move> ourMoves = MoveGenerator.GenerateLegalMoves(board);

        for (int i = 0; i < ourMoves.Count; i++)
        {
            // Find eval of this move
            board.MakeMove(ourMoves[i]);

            int eval = SearchMove(board, 3, int.MinValue, int.MaxValue);
            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = ourMoves[i];
            }

            board.UnMakeMove(ourMoves[i]);
        }

        return bestMove;
    }

    public int SearchMove(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
            return Evaluation.Evaluate(board);

        int bestEval = int.MinValue;
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);

        // Check for stalemate or checkmate
        if (moves.Count == 0)
        {
            // Treat them the same for now, but use checks later!
            return int.MinValue;
        }
        
        // Search moves
        for (int i = 0; i < moves.Count; i++)
        {
            board.MakeMove(moves[i]);
            int eval = -SearchMove(board, depth - 1, -beta, -alpha);
            board.UnMakeMove(moves[i]);

            if (eval > bestEval)
            {
                bestEval = eval;
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            if (eval >= beta)
                return bestEval;
            
            
        }

        return bestEval;
    }
}

