public struct GameState
{
    public bool WhiteKingsideCastle;
    public bool WhiteQueensideCastle;
    public bool BlackKingsideCastle;
    public bool BlackQueensideCastle;
    public int FiftyMoveCount;
    public int EnPassantSquare;

    public GameState(bool hasWhiteKingside, bool hasWhiteQueenside, bool hasBlackKingside, bool hasBlackQueenside, int fiftyMove, int enPassantSquare)
    {
        WhiteKingsideCastle = hasWhiteKingside;
        WhiteQueensideCastle = hasWhiteQueenside;
        BlackKingsideCastle = hasBlackKingside;
        BlackQueensideCastle = hasBlackQueenside;
        FiftyMoveCount = fiftyMove;
        EnPassantSquare = enPassantSquare;
    }

    public bool HasCastleRights(int color)
    {
        return color == Piece.White ?
            WhiteKingsideCastle | WhiteQueensideCastle:
            BlackKingsideCastle | BlackQueensideCastle;
    }

    public static bool IsEqual(GameState a, GameState b)
    {
        return a.WhiteKingsideCastle == b.WhiteKingsideCastle &&
            a.WhiteQueensideCastle == b.WhiteQueensideCastle &&
            a.BlackKingsideCastle == b.BlackKingsideCastle &&
            a.BlackQueensideCastle == b.BlackQueensideCastle &&
            a.FiftyMoveCount == b.FiftyMoveCount &&
            a.EnPassantSquare == b.EnPassantSquare;
    }

    public override string ToString()
    {
        string gameState = "";

        string castleRights = "";
        if (WhiteKingsideCastle) castleRights += "K";
        if (WhiteQueensideCastle) castleRights += "Q";
        if (BlackKingsideCastle) castleRights += "k";
        if (BlackQueensideCastle) castleRights += "q";
        gameState += castleRights + " ";

        gameState += FiftyMoveCount.ToString() + " ";
        if (EnPassantSquare != 0)
            gameState += BoardUtils.SquareToString(EnPassantSquare);
        
        return gameState;
    }
}

