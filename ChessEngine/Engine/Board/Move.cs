
using System.Diagnostics.CodeAnalysis;

public readonly struct Move
{
    // Struct members
    readonly ushort moveValue;
    readonly int lastCapturedPiece;

    // Static helpers
    public static readonly int[] OrthogonalOffsets = { 8, 1, -8, 1 };
    public static readonly int[] DiagonalOffsets = { 9, -7, -9, 7 };
    public static readonly int[] OmniOffsets = { 8, 1, -8, 1, 9, -7, -9, 7 };
    public static readonly int[] KnightOffsets = { 17, 10, -6, -15, -17, 10, 6, 15 };
    
    // Move type and flags (inform the board that a special case has happened!)
    public const int StandardMove = 0;
    public const int EnPassantCapture = 1;
    public const int Castle = 2;
    public const int PawnDoublePush = 3;

    public const int PromoteToQueen = 4;
    public const int PromoteToRook = 5;
    public const int PromoteToKnight = 6;
    public const int PromoteToBishop = 7;
    public const int PieceCapturedFlag = 8; // True bitflag

    const ushort startSquareMask    = 0b0000000000111111;
    const ushort targetSquareMask   = 0b0000111111000000;
    const ushort typeMask           = 0b0111000000000000;
    const ushort captureMask        = 0b1000000000000000;

    public Move(ushort moveValue)
    {
        this.moveValue = moveValue;
        lastCapturedPiece = Piece.None;
    }

    public Move(int startSquare, int targetSquare)
    {
        moveValue = (ushort) (startSquare | targetSquare << 6);
        lastCapturedPiece = Piece.None;
    }
    
    public Move(int startSquare, int targetSquare, int flag)
    {
        moveValue = (ushort) (startSquare | targetSquare << 6 | flag << 12);
        lastCapturedPiece = Piece.None;
    }

    public Move(int startSquare, int targetSquare, int flag, int lastCapturedPiece)
    {
        moveValue = (ushort) (startSquare | targetSquare << 6 | flag << 12);
        this.lastCapturedPiece = lastCapturedPiece;
    }

    public ushort Value => moveValue;
    
    public bool IsNull => moveValue == 0;
    
    public int StartSquare => moveValue & startSquareMask;
    
    public int TargetSquare => (moveValue & targetSquareMask) >> 6;
    
    public int MoveType => (moveValue & typeMask) >> 12;
    
    public bool IsCapture => (moveValue & captureMask) == captureMask;
    
    public bool IsPromotion => MoveType is PromoteToQueen or PromoteToRook or PromoteToKnight or PromoteToBishop;
    
    public bool IsPawnDoublePush => MoveType is PawnDoublePush;

    public bool IsCastle => MoveType is Castle;

    public bool IsEnpassantCapture => MoveType is EnPassantCapture;
    
    public int LastCapturedPiece => lastCapturedPiece;
    
    public int PromotionPieceType
    {
        get
        {
            switch (MoveType)
            {
                case PromoteToQueen: return Piece.Queen;
                case PromoteToKnight: return Piece.Knight;
                case PromoteToBishop: return Piece.Bishop;
                case PromoteToRook: return Piece.Rook;
                default: return Piece.None;
            }
        }
    }

    public string ToUCIMoveString()
    {
        string moveString = ToString().Substring(0, 4);
        if (IsPromotion)
        {
            moveString += PromotionPieceType switch
            {
                Piece.Queen => "q",
                Piece.Knight => "n",
                Piece.Bishop => "b",
                Piece.Rook => "r",
                _ => ""
            };
        }

        return moveString;
    }

    public override string ToString()
    {
        string moveString = BoardUtils.SquareToString(StartSquare) + BoardUtils.SquareToString(TargetSquare);

        // Promotion
        // if promotion
        string promotion;
        switch (MoveType)
        {
            case Move.PromoteToBishop:
                promotion = "b";
                break;
            case Move.PromoteToKnight:
                promotion = "n";
                break;
            case Move.PromoteToRook:
                promotion = "r";
                break;
            case Move.PromoteToQueen:
                promotion = "q";
                break;
            case Move.Castle: // yeah i know, doesn't really fit the names, give me a break and I'll fix it.
                promotion = "O-O";
                break;
            case Move.PawnDoublePush:
                promotion = "++";
                break;
            default:
                promotion = "";
                break;
        }

        return moveString + "" + promotion + "" + (IsCapture ? "X" : "");
    }

    public static Move NullMove = new Move(0);
    
    public static bool operator ==(Move a, Move b)
    {
        return a.moveValue == b.moveValue && a.lastCapturedPiece == b.lastCapturedPiece;
    }

    public static bool operator !=(Move a, Move b)
    {
        return a.moveValue != b.moveValue || a.lastCapturedPiece != b.lastCapturedPiece;
    }

    public override bool Equals(object? obj) => obj is Move m && this == m;

    public override int GetHashCode() => HashCode.Combine(moveValue, lastCapturedPiece);

}

