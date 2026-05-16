public static class MoveGenerator
{
    // Tables for move types
    static readonly ulong[,] PawnMoves = new ulong[2, 64]; // Moves of the pawn
    static readonly ulong[,] PawnAttacks = new ulong[2, 64]; // Captures of the pawn
    static readonly ulong[] KingAttacks = new ulong[64];
    static readonly ulong[] KnightAttacks = new ulong[64];

    // Useful bitboards for castling
    const ulong WhiteKingsideCastlePath = 0x60;
    const ulong WhiteQueensideCastlePath = 0xE;
    const ulong BlackKingsideCastlePath = 0x6000000000000000;
    const ulong BlackQueensideCastlePath = 0xE00000000000000;
    const ulong WhiteQueensideCastleAttacks = 0xC;
    const ulong BlackQueensideCastleAttacks = 0xC00000000000000;

    // State of the move generator
    public static bool InCheck { get; private set; }
    public static ulong OpponentAttacks {  get; private set; }
    static ulong attackingPieces;
    static int attackingPieceCount;
    static ulong checkRays; // Rays that attack the king
    static ulong pinRaysToKing; // Rays that attack the king, but are stopped by a friendly piece.
    
    static MoveGenerator()
    {
        PopulateMoveTables();
    }

    static void PopulateMoveTables()
    {
        // King attacks
        for (int i = 0; i < 64; i++)
        {
            ulong square = 1UL << i;

            KingAttacks[i] =
                square << 8 |
                square >> 8 |
                (square & ~Bitboard.FileH) << 1 |
                (square & ~Bitboard.FileA) >> 1 |
                (square & ~Bitboard.FileA) << 7 |
                (square & ~Bitboard.FileH) << 9 |
                (square & ~Bitboard.FileH) >> 7 |
                (square & ~Bitboard.FileA) >> 9;
        }

        // Knight attacks
        for (int i = 0; i < 64; i++)
        {
            ulong square = 1UL << i;
            KnightAttacks[i] =
                ((square & ~Bitboard.FileH) << 17 ) |
                ((square & ~(Bitboard.FileG | Bitboard.FileH)) << 10)  |
                ((square & ~(Bitboard.FileG | Bitboard.FileH)) >> 6 ) |
                ((square & ~Bitboard.FileH) >> 15 ) |
                ((square & ~Bitboard.FileA) << 15 ) |
                ((square & ~(Bitboard.FileA | Bitboard.FileB)) << 6 ) |
                ((square & ~(Bitboard.FileA | Bitboard.FileB)) >> 10)  |
                ((square & ~Bitboard.FileA) >> 17 );
        }

        // Pawn attacks
        for (int i = 8; i < 64; i++)
        {
            ulong square = 1UL << i;
            // White pawn moves
            PawnMoves[0, i] =
                Bitboard.North(square) | // North one
                Bitboard.North(Bitboard.North(square & Bitboard.Rank2)); // North two

            // White pawn captures
            PawnAttacks[0, i] =
                Bitboard.NorthEast(square) |
                Bitboard.NorthWest(square);
        }

        for (int i = 0; i < 56; i++)
        {
            ulong square = 1UL << i;
            // Black pawn moves
            PawnMoves[1, i] =
                Bitboard.South(square) |
                Bitboard.South(Bitboard.South(square & Bitboard.Rank7));

            // Black pawn captures
            PawnAttacks[1, i] =
                Bitboard.SouthEast(square) |
                Bitboard.SouthWest(square);
        }

    }

    // This function is not efficient AT ALL, but since it's only intended to be used for perft and debugging, it hardly matters.
    public static List<Move> GenerateLegalMoves(Board board, bool quietOnly = false, bool capturesOnly = false)
    {
        // Filter through these for illegal 
        List<Move> moves = new();
        List<Move> psuedolegalMoves = GeneratePsuedolegalMoves(board, quietOnly, capturesOnly);
        int friendlyColor = board.TurnColor; // Color of king to check attacks against
        int enemyColor = board.OpponentTurnColor;

        foreach (Move moveToVerify in psuedolegalMoves)
        {
            board.MakeMove(moveToVerify);

            ulong kingBitboard = Bitboard.SquareToBitboard(board.AllPieces[Piece.King | friendlyColor][0]);
            ulong opponentAttacks = GenerateAllAttacksBitboard(board, enemyColor); // Generate all attacks of the opponent

            // Check if any move would put king in check
            if ((opponentAttacks & kingBitboard) == 0)
            {
                // King is not attacked, add move
                moves.Add(moveToVerify);
            }

            board.UnMakeMove(moveToVerify);          
        }

        return moves;
    }

    public static List<Move> GeneratePsuedolegalMoves(Board board, bool quietOnly = false, bool capturesOnly = false)
    {
        List<Move> moves = new();

        // Iterate through each piece list
        int colorToMove = board.TurnColor;
        foreach (int square in board.AllPieces[Piece.Pawn | colorToMove])
            moves.AddRange(GeneratePawnMoves(board, square, quietOnly, capturesOnly));

        foreach (int square in board.AllPieces[Piece.Knight | colorToMove])
            moves.AddRange(GenerateKnightMoves(board, square, quietOnly, capturesOnly));

        foreach (int square in board.AllPieces[Piece.Bishop | colorToMove])
            moves.AddRange(GenerateSlidingMoves(board, square, quietOnly, capturesOnly));

        foreach (int square in board.AllPieces[Piece.Rook | colorToMove])
            moves.AddRange(GenerateSlidingMoves(board, square, quietOnly, capturesOnly));

        foreach (int square in board.AllPieces[Piece.Queen | colorToMove])
            moves.AddRange(GenerateSlidingMoves(board, square, quietOnly, capturesOnly));

        foreach (int square in board.AllPieces[Piece.King | colorToMove])
            moves.AddRange(GenerateKingMoves(board, square, quietOnly, capturesOnly));

        return moves;
    }

    public static List<Move> GenerateKingMoves(Board board, int location, bool quietOnly = false, bool capturesOnly = false)
    {
        List<Move> moves = new();

        // Useful info
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        int enemyColor = pieceColor == Piece.White ? Piece.Black : Piece.White;
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong enemyPieces = pieceColor == Piece.White ?
            board.GetBlackBitboard():
            board.GetWhiteBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();


        // Get attackable squares
        ulong moveBitboard = GenerateKingAttackBitboard(board, location);

        // Seperate captures from normal moves
        ulong attacks = Bitboard.Intersection(enemyPieces, moveBitboard);
        moveBitboard = Bitboard.Prune(moveBitboard, attacks);

        // Add each non-capture move
        while (moveBitboard != 0 && !capturesOnly)
        {
            int target = Bitboard.LSBToSquare(moveBitboard); // Get target square
            moves.Add(new Move(location, target));
            moveBitboard = Bitboard.PopLSB(moveBitboard); // Mark square as processed
        }

        // Add each capture move
        while (attacks != 0 && !quietOnly)
        {
            int target = Bitboard.LSBToSquare(attacks); // Get target square
            moves.Add(new Move(location, target, Move.PieceCapturedFlag, board.GetPiece(target)));
            attacks = Bitboard.PopLSB(attacks); // Mark square as processed
        }

        // Add castling moves
        // Determine if rights are had
        if (board.HasCastleRight(pieceColor) && !capturesOnly)
        {
            ulong attackedSquares = GenerateAllAttacksBitboard(board, enemyColor);
            ulong kingBitboard = Bitboard.SquareToBitboard(location);
            
            if (board.KingsideCastleRight(pieceColor))
            {
                ulong mask = pieceColor == Piece.White ? WhiteKingsideCastlePath : BlackKingsideCastlePath;

                // Check that squares aren't occupied, squares aren't under attack, nor in check
                if (Bitboard.Intersection(mask, occupiedSquares) == 0 &&
                    Bitboard.Intersection(mask, attackedSquares) == 0 &&
                    Bitboard.Intersection(attackedSquares, kingBitboard) == 0)
                {
                    // Add move
                    moves.Add(new Move(location, location + 2, Move.Castle));
                }
            }

            if (board.QueensideCastleRight(pieceColor))
            {
                ulong mask = pieceColor == Piece.White ? WhiteQueensideCastlePath : BlackQueensideCastlePath;
                ulong attackMask = pieceColor == Piece.White ? WhiteQueensideCastleAttacks : BlackQueensideCastleAttacks;

                // Check that squares aren't occupied, squares aren't under attack, nor in check
                if (Bitboard.Intersection(mask, occupiedSquares) == 0 &&
                    Bitboard.Intersection(attackMask, attackedSquares) == 0 &&
                    Bitboard.Intersection(attackedSquares, kingBitboard) == 0)
                {
                    // Add move
                    moves.Add(new Move(location, location - 2, Move.Castle));
                }
            }
                       
        }

        return moves;
    }

    public static List<Move> GenerateSlidingMoves(Board board, int location, bool quietOnly = false, bool capturesOnly = false)
    {
        List<Move> moves = new();

        // Useful info
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong enemyPieces = pieceColor == Piece.White ?
            board.GetBlackBitboard() :
            board.GetWhiteBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();

        // Get all viable moves
        ulong moveBitboard = GenerateSlidingAttackBitboard(board, location);

        // Seperate captures from normal moves
        ulong attacks = Bitboard.Intersection(enemyPieces, moveBitboard);
        moveBitboard = Bitboard.Prune(moveBitboard, attacks);

        // Convert to moves (unless searching for captures only)
        while (moveBitboard != 0 && !capturesOnly)
        {
            int target = Bitboard.LSBToSquare(moveBitboard);
            moves.Add(new Move(location, target));
            moveBitboard = Bitboard.PopLSB(moveBitboard);
        }

        // Convert to capture moves (unless we're doing a quiet search)
        while (attacks != 0 && !quietOnly) 
        {
            int target = Bitboard.LSBToSquare(attacks);
            moves.Add(new Move(location, target, Move.PieceCapturedFlag, board.GetPiece(target)));
            attacks = Bitboard.PopLSB(attacks);
        }

        return moves;
    }

    public static List<Move> GenerateKnightMoves(Board board, int location, bool quietOnly = false, bool capturesOnly = false)
    {
        List<Move> moves = new();

        // Useful info
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong enemyPieces = pieceColor == Piece.White ?
            board.GetBlackBitboard() :
            board.GetWhiteBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();

        // Get all viable moves
        ulong moveBitboard = GenerateKnightAttackBitboard(board, location);

        // Seperate captures from normal moves
        ulong attacks = Bitboard.Intersection(enemyPieces, moveBitboard);
        moveBitboard = Bitboard.Prune(moveBitboard, attacks);

        // Add each non-capture move
        while (moveBitboard != 0 && !capturesOnly)
        {
            int target = Bitboard.LSBToSquare(moveBitboard);
            moves.Add(new Move(location, target));
            moveBitboard = Bitboard.PopLSB(moveBitboard);
        }

        // Add each capture move
        while (attacks != 0 && !quietOnly)
        {
            int target = Bitboard.LSBToSquare(attacks);
            moves.Add(new Move(location, target, Move.PieceCapturedFlag, board.GetPiece(target)));
            attacks = Bitboard.PopLSB(attacks);
        }

        return moves;
    }

    public static List<Move> GeneratePawnMoves(Board board, int location, bool quietOnly = false, bool capturesOnly = false)
    {
        List<Move> moves = new();

        // Useful information
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong enemyPieces = pieceColor == Piece.White ?
            board.GetBlackBitboard() :
            board.GetWhiteBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();
        int colorIndex = pieceColor == Piece.White ? 0 : 1;

        // Get attacks
        ulong attacks = GeneratePawnAttackBitboard(board, location);

        // Prune of non-capture attacks
        attacks = Bitboard.Prune(attacks, ~occupiedSquares);

        // Generate push moves
        ulong moveBitboard = PawnMoves[colorIndex, location];

        // Check each available move for blockers
        int forward = pieceColor == Piece.White ? location + 8 : location - 8;
        ulong forwardMask = Bitboard.SquareToBitboard(forward);

        if ((occupiedSquares & forwardMask) != 0)
        {
            moveBitboard = 0;
        }
        else if (!capturesOnly) // Check double push ONLY if we're not searching for captures only.
        { // Possibly change this to mailbox approach, as this may be too much.
            moveBitboard = Bitboard.Intersection(moveBitboard, forwardMask);
            int doubleForward = pieceColor == Piece.White ? location + 16 : location - 16;
            if ((PawnMoves[colorIndex, location] & Bitboard.SquareToBitboard(doubleForward)) != 0)
            {
                ulong doubleMask = Bitboard.SquareToBitboard(doubleForward);

                // Push if squares are free
                if ((occupiedSquares & doubleMask) == 0)
                {
                    int target = Bitboard.LSBToSquare(doubleMask);
                    moves.Add(new Move(location, target, Move.PawnDoublePush));
                }
            }
        }
        

        // Add non-capture moves
        while (moveBitboard != 0 && !capturesOnly)
        {
            int target = Bitboard.LSBToSquare(moveBitboard);
            // Check if move targets the back rank
            if ((Bitboard.LSB(moveBitboard) & (Bitboard.Rank1 | Bitboard.Rank8)) != 0)
            {
                moves.Add(new Move(location, target, Move.PromoteToQueen));
                moves.Add(new Move(location, target, Move.PromoteToRook));
                moves.Add(new Move(location, target, Move.PromoteToBishop));
                moves.Add(new Move(location, target, Move.PromoteToKnight));
            }
            else
            {
                moves.Add(new Move(location, target));
            }
            moveBitboard = Bitboard.PopLSB(moveBitboard);
        }

        // Add capturing moves
        while (attacks != 0 && !quietOnly)
        {
            int target = Bitboard.LSBToSquare(attacks);
            // Check if move targets the back rank
            if ((Bitboard.LSB(attacks) & (Bitboard.Rank1 | Bitboard.Rank8)) != 0)
            {
                moves.Add(new Move(location, target, Move.PromoteToQueen | Move.PieceCapturedFlag, board.GetPiece(target)));
                moves.Add(new Move(location, target, Move.PromoteToRook | Move.PieceCapturedFlag, board.GetPiece(target)));
                moves.Add(new Move(location, target, Move.PromoteToBishop | Move.PieceCapturedFlag, board.GetPiece(target)));
                moves.Add(new Move(location, target, Move.PromoteToKnight | Move.PieceCapturedFlag, board.GetPiece(target)));
            }
            else
            {
                moves.Add(new Move(location, target, Move.PieceCapturedFlag, board.GetPiece(target)));
            }
            attacks = Bitboard.PopLSB(attacks);
        }

        // Handle special case of enpassant captures
        ulong enpassantAttacks = 0;
        if (board.GetEnpassantSquare != 0)
        {
            // Process en passant into an attack bitboard
            enpassantAttacks = Bitboard.Intersection(PawnAttacks[colorIndex, location], Bitboard.SquareToBitboard(board.GetEnpassantSquare));
        }        

        // Add enpassant moves
        while (enpassantAttacks != 0 && !quietOnly)
        {
            int target = Bitboard.LSBToSquare(enpassantAttacks);
            int pawnType = colorIndex == 0 ? Piece.BlackPawn : Piece.WhitePawn;
            moves.Add(new Move(location, target, Move.PieceCapturedFlag | Move.EnPassantCapture, pawnType));
            enpassantAttacks = Bitboard.PopLSB(enpassantAttacks);
        }
        
        return moves;
    }

    public static ulong GenerateAttackedBitboard(Board board)
    {
        ulong attacks = 0;

        for (int i = 0; i < 64; i++)
        {
            int piece = board.GetPiece(i);
            int pieceType = Piece.GetPieceType(piece);

            if (piece == 0) // Skip empty square
                continue;

            if (Piece.IsColor(piece, board.TurnColor)) // Skip non-opponent pieces
                continue;

            // Generate attack bitboard for this piece
            switch (pieceType)
            {
                case Piece.Pawn:
                {
                    attacks |= GeneratePawnAttackBitboard(board, i);
                    break;
                }
                case Piece.Knight:
                {
                    attacks |= GenerateKnightAttackBitboard(board, i);
                    break;
                }
                case Piece.King:
                {
                    attacks |= GenerateKingAttackBitboard(board, i);
                    break;
                }
                case Piece.Bishop:
                {
                    attacks |= GenerateSlidingAttackBitboard(board, i);
                    break;
                }
                case Piece.Rook:
                {
                    attacks |= GenerateSlidingAttackBitboard(board, i);
                    break;
                }
                case Piece.Queen:
                {
                    attacks |= GenerateSlidingAttackBitboard(board, i);
                    break;
                }
                default:
                    throw new InvalidDataException("Invalid piece type");

            }

        }

        return attacks;
    }

    public static ulong GenerateSlidingAttackBitboard(Board board, int location)
    {
        ulong attacks = 0;

        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        int enemyColor = Piece.GetOpponentPieceColor(piece);
        bool isDiag = Piece.IsDiagonalPiece(piece);
        bool isOrth = Piece.IsOrthogonalPiece(piece);
        ulong occupiedBitboard = board.GetOccupancyBitboard();
        ulong friendlyPieces = pieceColor == Piece.White ? board.GetWhiteBitboard() : board.GetBlackBitboard();

        if (isDiag)
            attacks |= MagicBitboard.GetDiagAttacks(location, occupiedBitboard);
        if (isOrth)
            attacks |= MagicBitboard.GetOrthoAttacks(location, occupiedBitboard);

        // Remove attacks on own pieces
        attacks = Bitboard.Prune(attacks, friendlyPieces);

        return attacks;
    }

    public static ulong GenerateKnightAttackBitboard(Board board, int location)
    {
        ulong attacks = 0;

        // Useful values
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();

        // Get all viable attacks
        attacks = KnightAttacks[location];

        // Prune of self-captures
        attacks = Bitboard.Prune(attacks, friendlyPieces);

        return attacks;
    }

    public static ulong GenerateKingAttackBitboard(Board board, int location)
    {
        ulong attacks = 0;

        // Useful values
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();

        // Get all viable attacks
        attacks = KingAttacks[location];

        // Prune of self-captures
        attacks = Bitboard.Prune(attacks, friendlyPieces);

        return attacks;
    }

    // DOES NOT ACCOUNT FOR ENPASSANT ATTACKS!
    public static ulong GeneratePawnAttackBitboard(Board board, int location)
    {
        ulong attacks = 0;

        // Useful values
        int piece = board.GetPiece(location);
        int pieceColor = Piece.GetPieceColor(piece);
        ulong friendlyPieces = pieceColor == Piece.White ?
            board.GetWhiteBitboard() :
            board.GetBlackBitboard();
        ulong occupiedSquares = board.GetOccupancyBitboard();
        int colorIndex = pieceColor == Piece.White ? 0 : 1;

        // Get all viable attacks
        attacks = PawnAttacks[colorIndex, location];

        // Prune of self captures
        attacks = Bitboard.Prune(attacks, friendlyPieces);

        return attacks;
    }

    public static ulong GenerateAllAttacksBitboard(Board board, int color)
    {
        ulong attacks = 0;

        foreach (int square in board.AllPieces[Piece.Pawn | color])
            attacks |= GeneratePawnAttackBitboard(board, square);

        foreach (int square in board.AllPieces[Piece.Knight | color])
            attacks |= GenerateKnightAttackBitboard(board, square);

        foreach (int square in board.AllPieces[Piece.Bishop | color])
            attacks |= GenerateSlidingAttackBitboard(board, square);

        foreach (int square in board.AllPieces[Piece.Rook | color])
            attacks |= GenerateSlidingAttackBitboard(board, square);

        foreach (int square in board.AllPieces[Piece.Queen | color])
            attacks |= GenerateSlidingAttackBitboard(board, square);

        foreach (int square in board.AllPieces[Piece.King | color])
            attacks |= GenerateKingAttackBitboard(board, square);

        return attacks;
    }

    static void UpdateAttackData(Board board)
    {

    }
}

