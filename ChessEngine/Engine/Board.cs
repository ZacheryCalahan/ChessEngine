using System.Drawing;

public class Board
{
    public bool IsWhiteTurn { get; private set; }
    public GameState CurrentGameState;
    Stack<GameState> oldGameStates;

    int[] board;
    ulong[] bitboards;
    ulong whitePieces = 0;
    ulong blackPieces = 0;

    PieceList[] AllPieces;

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

        AllPieces = new PieceList[Piece.PieceIndexCount];
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

        // Piece List
        for (int i = 0; i < Piece.PieceIndexCount; i++)
            AllPieces[i] = new PieceList(); // Reset piece lists

        for (int i = 0; i < 64; i++)
        {
            int piece = board[i];

            if (piece == 0)
                continue;

            AllPieces[piece].AddPiece(i);
        }
    }

    public void MakeMove(Move move)
    {
        // Helpful vars
        int movingPiece = board[move.StartSquare];
        int capturedPiece = board[move.TargetSquare];
        int capturedPieceType = Piece.GetPieceType(capturedPiece);
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int movingPieceType = Piece.GetPieceType(movingPiece);
        GameState newState = CurrentGameState;

        /* Handle promotions */
        if (move.IsPromotion)
        {
            // Replace the piece with the promotion piece
            bitboards[movingPiece] = Bitboard.RemoveSquare(bitboards[movingPiece], startSquare); // Remove piece from bitboard
            int promotePiece = TurnColor | move.PromotionPieceType;
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
        if (CurrentGameState.HasCastleRights(TurnColor))
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
                int rookPiece = TurnColor | Piece.Rook;
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
            int capturedPawn = OpponentTurnColor | Piece.Pawn;
            int capturedPieceLocation = GetEnpassantSquare + (IsWhiteTurn ? -8 : 8);

            bitboards[capturedPawn] = Bitboard.RemoveSquare(bitboards[capturedPawn], capturedPieceLocation);
            board[capturedPieceLocation] = Piece.None;
        }

        /* Update board states */

        // Update bitboards
        bitboards[movingPiece] = Bitboard.RemoveSquare(bitboards[movingPiece], startSquare); // Remove piece from the start square (moved piece)
        bitboards[movingPiece] = Bitboard.SetSquare(bitboards[movingPiece], targetSquare); // Place piece at target location.
        if (capturedPiece != 0) // Remove captured piece if capture
            bitboards[capturedPiece] = Bitboard.RemoveSquare(bitboards[capturedPiece], targetSquare);

        // Update mailbox
        board[targetSquare] = board[startSquare];
        board[startSquare] = Piece.None;

        // Update game state
        UpdateColorBitboards();
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
        int colorThatMoved = OpponentTurnColor;

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
            int capturedPawn = TurnColor | Piece.Pawn;
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

        

        // Update mailbox
        board[startSquare] = board[targetSquare];
        if (!move.IsEnpassantCapture)
            board[targetSquare] = capturedPiece;
        else
            board[targetSquare] = Piece.None;

        UpdateColorBitboards();

        /* Revert the board state */
        CurrentGameState = oldGameState;
        IsWhiteTurn = !IsWhiteTurn;

    }

    public int TurnColor => IsWhiteTurn ? Piece.White : Piece.Black;
    public int OpponentTurnColor => IsWhiteTurn ? Piece.Black : Piece.White;
    public int GetEnpassantSquare => CurrentGameState.EnPassantSquare;

    public bool HasCastleRight(int color) => CurrentGameState.HasCastleRights(color);

    public bool KingsideCastleRight(int color) => color == Piece.White ? CurrentGameState.WhiteKingsideCastle : CurrentGameState.BlackKingsideCastle;

    public bool QueensideCastleRight(int color) => color == Piece.White ? CurrentGameState.WhiteQueensideCastle : CurrentGameState.BlackQueensideCastle;
    
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

    /* Private helpers */
    void MovePiece(int piece, int startSquare, int targetSquare)
    {
        bitboards[piece] = Bitboard.RemoveSquare(bitboards[piece], startSquare);
        bitboards[piece] = Bitboard.SetSquare(bitboards[piece], targetSquare);

        board[startSquare] = Piece.None;
        board[targetSquare] = piece;

        AllPieces[piece].MovePiece(startSquare, targetSquare);
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

}

