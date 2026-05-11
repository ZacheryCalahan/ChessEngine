
using System.ComponentModel;

public static class Evaluation
{
    const int pawnValue = 100;
    const int knightValue = 320;
    const int bishopValue = 330;
    const int rookValue = 500;
    const int queenValue = 900;
    const int kingValue = 99999; // Because we could possibly use psudolegal moves, best to assign this an insane value. This naturally cancels out in eval unless it can be captured.

    public static int Evaluate(Board board, int whiteMoveCount = 0, int blackMoveCount = 0)
    {
        int eval = 0;

        // Start with just materials
        int friendlyEval = MaterialCount(board, board.TurnColor);
        int enemyEval = MaterialCount(board, board.OpponentTurnColor);

        bool isEnd = friendlyEval < (2100 + kingValue); // Consider end game when friendly piece count is low (I used one king, 4 pawns, and one queen as bounds)

        // Apply piece square table values
        friendlyEval += EvaluatePieceSquares(board, board.TurnColor, isEnd);
        enemyEval += EvaluatePieceSquares(board, board.OpponentTurnColor, isEnd);

        // Apply an incentive to finding positions with checks, and discourage positions that put you in check.
        if (board.IsInCheck())
        {
            enemyEval += 100;
            friendlyEval -= 100;
        }
        else if (board.IsEnemyInCheck())
        {
            friendlyEval += 100;
            enemyEval -= 100;
        }

        // Apply a mobility bonus (This should also double as good (and quick) way to measure the number of attacked squares)
        friendlyEval += (((board.TurnColor == Piece.White) ? whiteMoveCount : blackMoveCount) / 218) * 300; // Adds (moves / most legal moves) / 3x centipawn
        enemyEval += (((board.TurnColor == Piece.White) ? blackMoveCount : whiteMoveCount) / 218) * 300;

        // Return full eval
        eval = friendlyEval - enemyEval;
        return eval;
    }

    static int MaterialCount(Board board, int color)
    {
        int material = 0;

        // Iterate through each piece list
        double pawnCountScale = (board.AllPieces[Piece.Pawn | color].Count / 8.0);

        material += pawnValue * board.AllPieces[Piece.Pawn | color].Count;
        material += (int) (knightValue * board.AllPieces[Piece.Knight | color].Count * pawnCountScale); // Scale this back in value as pawns are removed
        material += bishopValue * board.AllPieces[Piece.Bishop | color].Count;
        material += (int) (rookValue * board.AllPieces[Piece.Rook | color].Count * (1 - pawnCountScale)); // Scale this forward in value as pawns are removed.
        material += queenValue * board.AllPieces[Piece.Queen | color].Count;
        material += kingValue * board.AllPieces[Piece.King | color].Count; // This is solely for pseudo legal moves, because we CAN capture kings that way.

        return material;
    }

    static int EvaluatePieceSquares(Board board, int color, bool isEnd = false)
    {
        int pieceSquareEval = 0;

        // Iterate through each piece list
        bool IsWhiteSide = color == Piece.White ? true : false;
        foreach (int square in board.AllPieces[Piece.Pawn | color])
            pieceSquareEval += PieceSquare.GetScore(isEnd ? PieceSquare.Pawns : PieceSquare.PawnsEnd, square, IsWhiteSide);

        foreach (int square in board.AllPieces[Piece.Knight | color])
            pieceSquareEval += PieceSquare.GetScore(PieceSquare.Knights, square, IsWhiteSide);

        foreach (int square in board.AllPieces[Piece.Bishop | color])
            pieceSquareEval += PieceSquare.GetScore(PieceSquare.Bishops, square, IsWhiteSide);

        foreach (int square in board.AllPieces[Piece.Rook | color])
            pieceSquareEval += PieceSquare.GetScore(PieceSquare.Rooks, square, IsWhiteSide);

        foreach (int square in board.AllPieces[Piece.Queen | color])
            pieceSquareEval += PieceSquare.GetScore(PieceSquare.Queens, square, IsWhiteSide);

        foreach (int square in board.AllPieces[Piece.King | color])
            pieceSquareEval += PieceSquare.GetScore(PieceSquare.Kings, square, IsWhiteSide);

        return pieceSquareEval;
    }
}

