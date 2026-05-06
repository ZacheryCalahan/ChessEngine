public class Board
{
    public bool IsWhiteTurn { get; private set; }
    public GameState CurrentGameState;

    Stack<GameState> oldGameStates;
    int[] board;
    ulong[] bitboards;
    ulong whitePieces = 0;
    ulong blackPieces = 0;

    public Board()
    {
        board = new int[64];
        bitboards = new ulong[Piece.MaxIndex + 1];
        for (int i = 0; i < Piece.MaxIndex; i++)
        {
            bitboards[i] = 0;
        }
        CurrentGameState = new();
        oldGameStates = new();
    }

    public Board(Board b)
    {
        IsWhiteTurn = b.IsWhiteTurn;
        CurrentGameState = b.CurrentGameState;
        board = new int[64];
        bitboards = new ulong[Piece.MaxIndex + 1];

        for (int i = 0; i < 64; i++)
        {
            board[i] = b.board[i];
        }

        for (int i = 0; i < Piece.MaxIndex + 1; i++)
        {
            bitboards[i] = b.bitboards[i];
        }
        
        oldGameStates = new();
    }

    /* Game state */

    public int GetTurnColor()
    {
        if (IsWhiteTurn)
            return Piece.White;

        return Piece.Black;
    }

    public int GetTurnOpponentColor()
    {
        if (IsWhiteTurn)
            return Piece.Black;

        return Piece.White;
    }
    
    public bool HasCastleRight(int color)
    {
        return CurrentGameState.HasCastleRights(color);
    }

    public bool KingsideCastleRight(int color)
    {
        return color == Piece.White ?
            CurrentGameState.WhiteKingsideCastle:
            CurrentGameState.BlackKingsideCastle;
    }

    public bool QueensideCastleRight(int color)
    {
        return color == Piece.White ?
            CurrentGameState.WhiteQueensideCastle :
            CurrentGameState.BlackQueensideCastle;
    }

    public void ImportBoard(string fen = FenUtils.StartPosFen)
    {
        // Position
        FenUtils.PositionInfo pos = FenUtils.SetupBoardFromFen(fen);
        board = pos.squares.ToArray();
        bitboards = pos.bitboards;
        UpdateColorBitboards();

        // Game state
        IsWhiteTurn = pos.whiteToMove;
        CurrentGameState = new GameState(
            pos.whiteCastleKingside,
            pos.whiteCastleQueenside,
            pos.blackCastleKingside,
            pos.blackCastleQueenside,
            pos.fiftyMoveCounter,
            pos.enPassantSquare);
        oldGameStates = new();
    }

    public int GetEnpassantSquare()
    {
        return CurrentGameState.EnPassantSquare;
    }

    /* Moves */

    public void MakeMove(Move move)
    {
        // Helpful vars
        int movingPiece = board[move.StartSquare];
        int capturingPiece = board[move.TargetSquare];
        int capturedPieceType = Piece.GetPieceType(capturingPiece);
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int movingPieceType = Piece.GetPieceType(movingPiece);
        GameState newState = CurrentGameState;

        /* Handle promotions */
        if (move.IsPromotion)
        {
            // Replace the piece with the promotion piece
            bitboards[movingPiece] = Bitboard.RemoveSquare(bitboards[movingPiece], startSquare); // Remove piece from bitboard
            int promotePiece = GetTurnColor() | move.PromotionPieceType;
            bitboards[promotePiece] = Bitboard.SetSquare(bitboards[promotePiece], startSquare); // Add piece at position for promotion type
            board[startSquare] = promotePiece;

            // Mark the moving piece as promotion piece
            movingPiece = promotePiece;
        }

        /* Handle pawn up two */
        if (move.IsPawnDoublePush)
        {
            newState.EnPassantSquare = IsWhiteTurn ?
                targetSquare - 8 :// White is negative dir
                targetSquare + 8; // Black is positive dir
        }
        else
        {
            newState.EnPassantSquare = 0;
        }

        /* Handle castling (and rights!) */
        if (CurrentGameState.HasCastleRights(GetTurnColor()))
        {
            if (move.IsCastle)
            {
                // Clear castle rights of this color
                if (IsWhiteTurn)
                {
                    newState.WhiteQueensideCastle = false;
                    newState.WhiteKingsideCastle = false;
                }
                else
                {
                    newState.BlackKingsideCastle = false;
                    newState.BlackQueensideCastle = false;
                }

                // Move the rook to the correct square
                int rookTarget = 0;
                int rookStart = 0;
                int rookPiece = GetTurnColor() | Piece.Rook;
                switch (targetSquare)
                {
                    case 6: // White Kingside
                    {
                        rookTarget = 5;
                        rookStart = 7;
                        break;
                    }
                    case 2: // White Queenside
                    {
                        rookTarget = 3;
                        rookStart = 0;
                        break;
                    }
                    case 58: // Black Queenside
                    {
                        rookTarget = 59;
                        rookStart = 56;
                        break;
                    }
                    case 62: // Black Kingside
                    {
                        rookTarget = 61;
                        rookStart = 63;
                        break;
                    }
                    default:
                    {
                        throw new InvalidDataException("Castle moved king to wrong square");
                    }

                }
                
                bitboards[rookPiece] = Bitboard.RemoveSquare(bitboards[rookPiece], rookStart); // Remove rook from start
                bitboards[rookPiece] = Bitboard.SetSquare(bitboards[rookPiece], rookTarget); // Place at target
                board[rookTarget] = rookPiece;
                board[rookStart] = Piece.None;
            }
            else if (movingPieceType == Piece.King) // Clear castle rights of this color
            {
                
                if (IsWhiteTurn)
                {
                    newState.WhiteQueensideCastle = false;
                    newState.WhiteKingsideCastle = false;
                }
                else
                {
                    newState.BlackKingsideCastle = false;
                    newState.BlackQueensideCastle = false;
                }
            }
            else if (movingPieceType == Piece.Rook) // Clear castle rights of this side in this color
            { 
                if (IsWhiteTurn)
                {
                    if (startSquare == 7)
                        newState.WhiteKingsideCastle = false;
                    else if (startSquare == 0)
                        newState.WhiteQueensideCastle = false;
                }
                else
                {
                    if (startSquare == 56)
                        newState.BlackQueensideCastle = false;
                    else if (startSquare == 63)
                        newState.BlackKingsideCastle = false;
                }
            }
        }

        // Remove rights is rook was captured on its start square
        if (capturedPieceType == Piece.Rook)
        {
            // Check which rook was taken, and remove those rights
            switch (targetSquare)
            {
                case 63:
                    newState.BlackKingsideCastle = false;
                    break;
                case 56:
                    newState.BlackQueensideCastle = false;
                    break;
                case 7:
                    newState.WhiteKingsideCastle = false;
                    break;
                case 0:
                    newState.WhiteQueensideCastle = false;
                    break;
                default:
                    break;
            }
        }

        /* Handle enpassant capture */
        if (move.IsEnpassantCapture)
        {
            // Remove the captured pawn
            int capturedPiece = GetTurnOpponentColor() | Piece.Pawn;
            int capturedPieceLocation = GetEnpassantSquare() + (IsWhiteTurn ? -8 : 8);

            bitboards[capturedPiece] = Bitboard.RemoveSquare(bitboards[capturedPiece], capturedPieceLocation);
            board[capturedPieceLocation] = Piece.None;
        }

        /* Update board states */

        // Update bitboards
        bitboards[movingPiece] = Bitboard.RemoveSquare(bitboards[movingPiece], startSquare); // Remove piece from the start square (moved piece)
        bitboards[movingPiece] = Bitboard.SetSquare(bitboards[movingPiece], targetSquare); // Place piece at target location.
        if (capturingPiece != 0) // Remove captured piece if capture
            bitboards[capturingPiece] = Bitboard.RemoveSquare(bitboards[capturingPiece], targetSquare); 
        UpdateColorBitboards();

        // Update mailbox
        board[targetSquare] = board[startSquare];
        board[startSquare] = Piece.None;

        // Update game state
        IsWhiteTurn = !IsWhiteTurn;

        if (movingPieceType != Piece.Pawn | move.IsCapture)
            newState.FiftyMoveCount++;
        else
            newState.FiftyMoveCount = 0;

        // Save the old game state so we can revert on unmake move
        oldGameStates.Push(CurrentGameState);
        CurrentGameState = newState;
    }

    public void UnMakeMove(Move move)
    {
        // Helpful vars
        int movedPiece = board[move.TargetSquare]; // Piece that is moving back to original position
        int capturedPiece = move.LastCapturedPiece;
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int colorThatMoved = GetTurnOpponentColor();

        GameState oldGameState = oldGameStates.Pop();

        /* Handle promotions */
        if (move.IsPromotion)
        {
            // Replace promoted piece with its pawn
            bitboards[movedPiece] = Bitboard.RemoveSquare(bitboards[movedPiece], targetSquare); // Remove promoted piece
            int pawn = colorThatMoved | Piece.Pawn;
            bitboards[pawn] = Bitboard.SetSquare(bitboards[pawn], targetSquare);
            board[targetSquare] = pawn;
            movedPiece = pawn;
        }

        // Don't handle the enpassant square, we're just returning it's state from the old state.

        /* Handle castles */
        if (move.IsCastle)
        {
            // Don't worry about castling rights, that's returned from old state.
            int rookTarget = 0;
            int rookStart = 0;
            int rookPiece = colorThatMoved | Piece.Rook;
            switch (targetSquare)
            {
                case 6: // White Kingside
                {
                    rookTarget = 5;
                    rookStart = 7;
                    break;
                }
                case 2: // White Queenside
                {
                    rookTarget = 3;
                    rookStart = 0;
                    break;
                }
                case 58: // Black Queenside
                {
                    rookTarget = 59;
                    rookStart = 56;
                    break;
                }
                case 62: // Black Kingside
                {
                    rookTarget = 61;
                    rookStart = 63;
                    break;
                }
                default:
                {
                    throw new InvalidDataException("Castle moved king to wrong square");
                }

            }

            // Return rook back to its position
            bitboards[rookPiece] = Bitboard.RemoveSquare(bitboards[rookPiece], rookTarget);
            bitboards[rookPiece] = Bitboard.SetSquare(bitboards[rookPiece], rookStart);
            board[rookTarget] = Piece.None;
            board[rookStart] = rookPiece;
        }
    
        /* Handle enpassant capture */
        if (move.IsEnpassantCapture)
        {
            // Replace the captured pawn
            int capturedPawn = GetTurnColor() | Piece.Pawn;
            int capturedPawnLocation = oldGameState.EnPassantSquare + (Piece.GetPieceColor(capturedPawn) == Piece.White ? 8 : -8);

            bitboards[capturedPawn] = Bitboard.SetSquare(bitboards[capturedPawn], capturedPawnLocation);
            board[capturedPawnLocation] = capturedPawn;
        }

        /* Revert piece movement */
        // Update bitboards
        bitboards[movedPiece] = Bitboard.RemoveSquare(bitboards[movedPiece], targetSquare); // Remove piece from target square
        bitboards[movedPiece] = Bitboard.SetSquare(bitboards[movedPiece], startSquare); // Place piece at target location.
        if (!move.IsEnpassantCapture && capturedPiece != 0) // Don't apply a pawn to the captured square if enpass, because the target is not where it was.
        {
            bitboards[capturedPiece] = Bitboard.SetSquare(bitboards[capturedPiece], targetSquare); // Replace captured piece
        }
        
        UpdateColorBitboards();

        // Update mailbox
        board[startSquare] = board[targetSquare];
        if (!move.IsEnpassantCapture)
            board[targetSquare] = capturedPiece;
        else
            board[targetSquare] = Piece.None;

        /* Revert the board state */
        CurrentGameState = oldGameState;
        IsWhiteTurn = !IsWhiteTurn;
    
    }

    void UpdateColorBitboards()
    {
        ulong bb = 0;

        for (int i = Piece.WhitePawn; i < 7; i++)
        {
            bb |= bitboards[i];
        }

        whitePieces = bb;
        bb = 0;

        for (int i = Piece.BlackPawn; i < Piece.PieceIndexCount; i++)
        {
            bb |= bitboards[i];
        }
        blackPieces = bb;

    }

    /* Piece retrieval */

    public int GetPiece(int square)
    {
        return board[square];
    }

    public ulong GetBitboard(int piece)
    {
        return bitboards[piece];
    }

    public ulong GetFriendlyBitboard()
    {
        if (IsWhiteTurn)
            return whitePieces;

        return blackPieces;
    }

    public ulong GetOpponentBitboard()
    {
        if (IsWhiteTurn)
            return blackPieces;

        return whitePieces;
    }

    public ulong GetWhiteBitboard()
    {
        return whitePieces;
    }

    public ulong GetBlackBitboard()
    {
        return blackPieces;
    }

    public ulong GetOccupancyBitboard()
    {
        return whitePieces | blackPieces;
    }


    public static bool IsEqual(Board a, Board b)
    {
        // Compare boards
        for (int i = 0; i < 64; i++)
        {
            if (a.board[i] != b.board[i])
                return false;
        }

        // Compare bitboards (start with White.Pawn, because index 0 is full of junk regardless.)
        for (int i = 1; i < a.bitboards.Length; i++)
        {
            if (a.bitboards[i] != b.bitboards[i])
                return false;
        }

        // Compare gamestate
        if (a.IsWhiteTurn != b.IsWhiteTurn)
            return false;



        return true;
    }

    
}

