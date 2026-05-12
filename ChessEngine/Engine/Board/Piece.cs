public static class Piece
{
    // Piece Types
    public const int None = 0;
    public const int Pawn = 1;      // Special movement
    public const int Knight = 2;    // Special movement
    public const int King = 3;      // Diag & Orth
    public const int Bishop = 4;    // Diagonal Slider
    public const int Rook = 5;      // Orthogonal Slider
    public const int Queen = 6;     // Diag & Orth Slider

    // Color
    public const int White = 0;
    public const int Black = 8;

    // Pieces
    public const int WhitePawn = Pawn | White;      // 1
    public const int WhiteKnight = Knight | White;  // 2
    public const int WhiteKing = King | White;      // 3
    public const int WhiteBishop = Bishop | White;  // 4
    public const int WhiteRook = Rook | White;      // 5
    public const int WhiteQueen = Queen | White;    // 6

    public const int BlackPawn = Pawn | Black;      // 9
    public const int BlackKnight = Knight | Black;  // 10
    public const int BlackKing = King | Black;      // 11
    public const int BlackBishop = Bishop | Black;  // 12
    public const int BlackRook = Rook | Black;      // 13
    public const int BlackQueen = Queen | Black;    // 14
    public const int MaxIndex = BlackQueen;
    public const int PieceIndexCount = MaxIndex + 1; // Used for bitboards[] serialization

    // Masks
    public const int PieceTypeMask = 0b0111;
    public const int ColorMask = 0b1000;

    public static int GetPieceType(int piece) => piece & PieceTypeMask;

    public static bool IsPieceType(int piece, int pieceType) => GetPieceType(piece) == pieceType;

    public static int GetPieceColor(int piece) => piece == 0 ? -1 : piece & ColorMask;

    public static bool IsWhite(int piece) => IsColor(piece, White);

    public static bool IsColor(int piece, int color) => (piece & ColorMask) == color && piece != 0;

    public static bool IsSlidingPiece(int piece) => (piece & PieceTypeMask) is Bishop or Rook or Queen;

    public static bool IsOrthogonalPiece(int piece) => (piece & PieceTypeMask) is Rook or Queen or King;

    public static bool IsDiagonalPiece(int piece) => (piece & PieceTypeMask) is Queen or King or Bishop;

    public static int GetOpponentPieceColor(int piece) => GetPieceColor(piece) == White ? Black : White;

    public static char ToChar(int piece)
    {
        string pieceList = " PNKBRQ??pnkbrq";
        return pieceList.ToCharArray()[piece];
    }

    public static string ToUnicode(int piece) {
        string[] pieceArray = { " ", "\u2659\uFE0E", "♘", "♔", "♗", "♖", "♕", "?", "?", "\u265F\uFE0E", "♞", "♚", "♝", "♜", "♛"};

        return pieceArray[piece];
    }

    public static int FromChar(char pieceLetter)
    {
        int piece = 0;

        bool isUpper = Char.IsUpper(pieceLetter);
        pieceLetter = Char.ToLower(pieceLetter);

        switch (pieceLetter)
        {
            case ('p'):
                piece |= Pawn;
                break;
            case ('n'):
                piece |= Knight;
                break;
            case ('b'):
                piece |= Bishop;
                break;
            case ('r'):
                piece |= Rook;
                break;
            case ('q'):
                piece |= Queen;
                break;
            case ('k'):
                piece |= King;
                break;
            default:
                return 0; // No piece :D
        }

        if (!isUpper)
            piece |= Black;

        return piece;
    }

    public static bool IsNull(int piece) => piece == 0;

    public static int MakePiece(int pieceType, int pieceColor) => pieceType | pieceColor;

}
