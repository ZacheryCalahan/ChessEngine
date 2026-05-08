
public static class Evaluation
{
    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;
    const int kingValue = 9999999; // Because we use psudolegal moves, best to assign this an insane value.

    public static int Evaluate(Board board)
    {
        int eval = 0;

        // Start with just materials
        int friendlyEval = MaterialCount(board, board.TurnColor);
        int enemyEval = MaterialCount(board, board.OpponentTurnColor);

        bool isEnd = friendlyEval < (700 + kingValue);

        // Apply piece square table values
        friendlyEval *= EvaluatePieceSquares(board, board.TurnColor, isEnd);
        enemyEval *= EvaluatePieceSquares(board, board.OpponentTurnColor, isEnd);
        
        // Return full eval
        eval = friendlyEval - enemyEval;
        return eval;
    }

    static int MaterialCount(Board board, int color)
    {
        int material = 0;

        for (int i = 0; i < 64; i++)
        {
            // Search board for pieces
            int piece = board.GetPiece(i);
            int pieceColor = Piece.GetPieceColor(piece);
            int pieceType = Piece.GetPieceType(piece);

            if (piece == Piece.None)
                continue;

            if (pieceColor != color)
                continue;

            material += pieceType switch
            {
                Piece.Pawn => pawnValue,
                Piece.Knight => knightValue,
                Piece.Bishop => bishopValue,
                Piece.Rook => rookValue,
                Piece.Queen => queenValue,
                Piece.King => kingValue,
                _ => 0
            };
        }

        return material;
    }

    static int EvaluatePieceSquares(Board board, int color, bool isEnd = false)
    {
        int pieceSquareEval = 0;

        for (int i = 0; i < 64; i++)
        {
            // Search board for pieces
            int piece = board.GetPiece(i);
            int pieceColor = Piece.GetPieceColor(piece);
            int pieceType = Piece.GetPieceType(piece);
            bool isWhite = pieceColor == Piece.White;

            if (piece == Piece.None)
                continue;

            if (pieceColor != color)
                continue;

            pieceSquareEval += pieceType switch
            {
                Piece.Pawn =>   PieceSquare.GetScore(isEnd ? PieceSquare.Pawns : PieceSquare.PawnsEnd, i, isWhite),
                Piece.Knight => PieceSquare.GetScore(PieceSquare.Knights, i, isWhite),
                Piece.Bishop => PieceSquare.GetScore(PieceSquare.Bishops, i, isWhite),
                Piece.Rook =>   PieceSquare.GetScore(PieceSquare.Rooks, i, isWhite),
                Piece.Queen =>  PieceSquare.GetScore(PieceSquare.Queens, i, isWhite),
                Piece.King =>   PieceSquare.GetScore(PieceSquare.Kings, i, isWhite),
                _ => 0
            };

        }

        return pieceSquareEval;
    }
}

