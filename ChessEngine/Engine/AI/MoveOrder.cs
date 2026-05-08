
public static class MoveOrder
{
    public static void OrderMoves(Board board, ref List<Move> moves)
    {
        // Save the score for each move for sorting
        List<int> scores = new(moves.Count);

        // Generate a score for each move
        foreach (Move move in moves)
        {
            int moveScore = 0;
            int movePieceType = Piece.GetPieceType(board.GetPiece(move.StartSquare));
            int capturePieceType = Piece.GetPieceType(board.GetPiece(move.TargetSquare));

            // Capture better pieces with worse pieces
            if (capturePieceType != Piece.None)
            {
                moveScore += 10 * GetPieceValue(capturePieceType) - GetPieceValue(movePieceType);
            }

            // Promote pawns
            if (move.IsPromotion)
            {
                moveScore += GetPieceValue(move.PromotionPieceType);
            }

            scores.Add(moveScore);

        }

        // Sort the moves by the score
        moves = moves
            .Select((m, i) => (Move: m, Score: scores[i]))
            .OrderByDescending(x => x.Score)
            .Select(x => x.Move)
            .ToList();
    }

    public static int GetPieceValue(int piece)
    {
        piece = Piece.GetPieceType(piece);

        return piece switch
        {
            Piece.Pawn => 100,
            Piece.Knight => 300,
            Piece.Bishop => 301,
            Piece.Rook => 500,
            Piece.Queen => 900,
            Piece.King => 9999999,
            _ => 0
        };
    }
}

