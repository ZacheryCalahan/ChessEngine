
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
        return MaterialCount(board);
    }

    static int MaterialCount(Board board)
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

            if (pieceColor != board.GetTurnColor())
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
}

