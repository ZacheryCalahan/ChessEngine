public static class UCI
{
    static Board CurrentBoard = new();

    public static void Start()
    {
        // Notify GUI we're in UCI mode
        GiveId();
        Init();

        // Command parsing
        while (true)
        {
            string? command = Console.ReadLine();
            if (command == null)
                continue;

            string messageType = command.Split(' ')[0].ToLower();

            switch (messageType)
            {
                case "uci": GiveId(); break;
                case "isready": Console.WriteLine("readyok"); break;
                case "position": HandlePosition(command); break;
                case "ucinewgame": CurrentBoard.ImportBoard(); break;
                case "debug": break;
                case "setoption": break;
                case "register": break;
                case "stop": break;
                case "p": BoardUtils.PrintBoardChar(CurrentBoard); break;
                case "perft": PrintPerft(command); break;
                case "quit": return; // Exit program
                case "go": HandleGo(command); break;
                _: continue;
            }
        }
    }

    static void GiveId()
    {
        Console.WriteLine("id name Zac's Chess Bot");
        Console.WriteLine("id author Zac Calahan");
        Console.WriteLine("uciok");
    }

    static void Init()
    {
        CurrentBoard = new();
    }

    static void HandlePosition(string message)
    {
        if (message.ToLower().Contains("startpos"))
        {
            CurrentBoard.ImportBoard();
        } 
        else if (message.ToLower().Contains("fen"))
        {
            CurrentBoard.ImportBoard(ParseFen(message));
        }

        string movestring;
        bool parsed = GetMoves(message, out movestring);
        if (parsed)
        {
            string[] moves = movestring.Split(' ');
            foreach (string move in moves)
            {
                MakeMove(move);
            }
        }
        else if (movestring != "none")
        {
            Console.WriteLine("Invalid position command.");
        }
    }

    static void PrintPerft(string message)
    {
        PerftUtils perft = new();
        int depth = 0;
        string depthStr = "";
        try
        {
            depthStr = message.Split(' ')[1];
        }
        catch (Exception e)
        {
            Console.WriteLine("Invalid depth argument.");
            return;
        }

        if (!int.TryParse(message.Split(' ')[1], out depth))
        {
            Console.WriteLine("Invalid depth argument.");
            return;
        }

        perft.PrintPerftTestDivide(CurrentBoard, depth);
    }

    static bool GetMoves(string message, out string moves)
    {
        message = message.Trim();
        if (message.IndexOf("moves") == -1)
        {
            moves = "none";
            return false;
        }

        int valueStart = message.IndexOf("moves") + "moves".Length; // Find end of moves command
        int valueEnd = message.Length;
        moves = message.Substring(valueStart, valueEnd - valueStart).Trim();
        return true;
    }

    static void MakeMove(string moveString)
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

        int movedPieceType = Piece.GetPieceType(CurrentBoard.GetPiece(startSquare));
        int movedPiece = CurrentBoard.GetPiece(startSquare);

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
            else if (startSquare % 8 != targetSquare % 8 && CurrentBoard.GetPiece(targetSquare) == Piece.None)
            {
                flag = Move.EnPassantCapture | Move.PieceCapturedFlag;
                pieceCaptured = Piece.Pawn | Piece.GetOpponentPieceColor(movedPiece);
            }
        }
        else if (movedPieceType == Piece.King)
        {
            if (Math.Abs(BoardUtils.FileIndex(startSquare) - BoardUtils.FileIndex(targetSquare)) > 1) {
                flag = Move.Castle;
            }
        }

        // Mark captures, and their piece.
        if (CurrentBoard.GetPiece(targetSquare) != Piece.None)
        {
            flag |= Move.PieceCapturedFlag;
            pieceCaptured = CurrentBoard.GetPiece(targetSquare);
        }
        
        Move move = new(startSquare, targetSquare, flag, pieceCaptured);
        CurrentBoard.MakeMove(move);
    }

    static void HandleGo(string message)
    {
        if (message.Contains("time"))
        {
            // Parse and pass time herew
            Bot.ThinkTimed(CurrentBoard, 1000);
        }
        else
        {
            // Assume infinite, think for however long you want!
            Bot.ThinkTimed(CurrentBoard, int.MaxValue);
        }

        Move move = Bot.BestMove();

        string moveString = move.ToString().Substring(0, 4);
        Console.WriteLine($"bestmove {moveString}");
    }

    static void HandleStop(string message)
    {
        Bot.StopThinking();
    }

    static string ParseFen(string message)
    {
        try
        {
            // This is dirty, but it does work.
            string[] tokens = message.Split(' ');
            string fen = tokens[2];
            fen += " " + tokens[3] + " " +
                tokens[4] + " " +
                tokens[5] + " " +
                tokens[6] + " " +
                tokens[7];

            return fen;

        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Invalid FEN string passed. Using startpos.");
            return FenUtils.StartPosFen;
        }
    }

    

}