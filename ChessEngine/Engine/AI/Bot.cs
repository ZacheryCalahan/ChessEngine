public class Bot
{
    public event Action<string>? OnMoveChosen;
    public Board board = new();

    // Search thread stuffs
    readonly AutoResetEvent searchWaitHandle;
    CancellationTokenSource? cancelSearchTimer;
    readonly Search searcher;
    public bool IsThinking { get; private set; }

    // Consts
    const int maxThinkTimeMs = 3000; // Allow up to 3 seconds of thinking time max

    // State
    bool IsQuitting = false;
    int currentSearchID = 0;

    public Bot()
    {
        searcher = new(board);
        searcher.OnSearchComplete += OnSearchComplete;

        searchWaitHandle = new(false);
        Task.Factory.StartNew(SearchThread, TaskCreationOptions.LongRunning);
    }

    public void NewBoard(string fen = FenUtils.StartPosFen)
    {
        while (IsThinking)
        { } // Hang until ready to mutate the board

        board.ImportBoard(fen);
    }

    public void MakeMove(string moveString)
    {
        int startSquare = 0;
        int targetSquare = 0;

        try
        {
            startSquare = BoardUtils.StringToSquare(moveString.Substring(0, 2));
            targetSquare = BoardUtils.StringToSquare(moveString.Substring(2, 2));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Invalid move: {moveString}");
            return;
        }

        int movedPieceType = Piece.GetPieceType(board.GetPiece(startSquare));
        int movedPiece = board.GetPiece(startSquare);

        // Determine the move flags
        int pieceCaptured = Piece.None;
        int flag = Move.StandardMove;

        if (movedPieceType == Piece.Pawn)
        {
            // Check promotion
            if (moveString.Length > 4)
            {
                flag = moveString[^1] switch
                {
                    'q' => Move.PromoteToQueen,
                    'r' => Move.PromoteToRook,
                    'n' => Move.PromoteToKnight,
                    'b' => Move.PromoteToBishop,
                    _ => Move.StandardMove
                };
            }
            // Double push
            else if (startSquare - targetSquare == 16 || startSquare - targetSquare == -16)
            {
                flag = Move.PawnDoublePush;
            }
            // Enpassant capture (moved to different file, but targeted no piece)
            else if (startSquare % 8 != targetSquare % 8 && board.GetPiece(targetSquare) == Piece.None)
            {
                flag = Move.EnPassantCapture | Move.PieceCapturedFlag;
                pieceCaptured = Piece.Pawn | Piece.GetOpponentPieceColor(movedPiece);
            }
        }
        else if (movedPieceType == Piece.King)
        {
            if (Math.Abs(BoardUtils.FileIndex(startSquare) - BoardUtils.FileIndex(targetSquare)) > 1)
            {
                flag = Move.Castle;
            }
        }

        // Mark captures, and their piece.
        if (board.GetPiece(targetSquare) != Piece.None)
        {
            flag |= Move.PieceCapturedFlag;
            pieceCaptured = board.GetPiece(targetSquare);
        }

        Move move = new(startSquare, targetSquare, flag, pieceCaptured);
        board.MakeMove(move);
    }

    public int DetermineThinkTime(int wtime, int btime, int winc, int binc)
    {
        int myTime = board.IsWhiteTurn ? wtime : btime;
        int myTimeInc = board.IsWhiteTurn ? winc : binc;

        // Use slice of time
        double thinkTimeMs = myTime / 40.0;

        // Base time
        thinkTimeMs = Math.Min(maxThinkTimeMs, thinkTimeMs);

        return (int) thinkTimeMs; 
        
    }

    public void ThinkTimed(int timeMs)
    {
        IsThinking = true;
        cancelSearchTimer?.Cancel();
        StartSearch(timeMs);
    }

    public void ThinkInfinite()
    {
        IsThinking = true;
        cancelSearchTimer?.Cancel();

        StartSearch(maxThinkTimeMs);
        
        
    }

    void StartSearch(int timeMs)
    {
        currentSearchID++;
        searchWaitHandle.Set();
        cancelSearchTimer = new CancellationTokenSource();
        Task.Delay(timeMs, cancelSearchTimer.Token).ContinueWith((t) => EndSearch(currentSearchID));
    }

    void EndSearch(int searchID)
    {
        if (cancelSearchTimer != null && cancelSearchTimer.IsCancellationRequested)
        {
            return;
        }

        if (currentSearchID == searchID)
        {
            StopThinking();
        }
    }

    public void StopThinking() 
    {
        cancelSearchTimer?.Cancel();
        if (IsThinking)
        {
            searcher.StopSearch();
        }
    }

    public void OnSearchComplete(Move move)
    {
        IsThinking = false;
        OnMoveChosen?.Invoke(move.ToUCIMoveString());
        
    }

    void SearchThread()
    {
        while (!IsQuitting)
        {
            searchWaitHandle.WaitOne();
            searcher.StartSearchDeepening();
        }
    }

}

