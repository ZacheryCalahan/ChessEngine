/*
    UCI, Bot, and Search borrow heavily from SebLague, mostly of the threading/task code to make sure everything was done right.
 */

public static class UCI
{
    
    static Bot bot;
    static UCI() {
        bot = new();
        bot.OnMoveChosen += OnMoveChosen;
    }

    public static void Start()
    {
        // Notify GUI we're in UCI mode
        GiveId();

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
                case "ucinewgame": bot.board.ImportBoard(); break;
                case "debug": break;
                case "setoption": break;
                case "register": break;
                case "stop": HandleStop(); break;
                case "p": BoardUtils.PrintBoardChar(bot.board); break;
                case "perft": PrintPerft(command); break;
                case "bb": BoardUtils.PrintAllBitboards(bot.board); break;
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

    static void HandlePosition(string message)
    {
        if (message.ToLower().Contains("startpos"))
        {
            bot.NewBoard();
        } 
        else if (message.ToLower().Contains("fen"))
        {
            bot.NewBoard(ParseFen(message));
        }

        string movestring;
        bool parsed = GetMoves(message, out movestring);
        if (parsed)
        {
            string[] moves = movestring.Split(' ');
            foreach (string move in moves)
            {
                bot.MakeMove(move);
            }
        }
        else if (movestring != "none")
        {
            Console.WriteLine("Invalid position command.");
        }
    }

    static void PrintPerft(string message)
    {
        // This will only ever be called out of search, do not worry of mutation.
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

        perft.PrintPerftTestDivide(bot.board, depth);
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

    static void HandleGo(string message)
    {
        if (message.Contains("time"))
        {
            // Parse and pass time here
            int wtime = TryGetLabeledInt(message, "wtime");
            int btime = TryGetLabeledInt(message, "btime");
            int winc = TryGetLabeledInt(message, "winc");
            int binc = TryGetLabeledInt(message, "binc");

            int thinkTime = bot.DetermineThinkTime(wtime, btime, winc, binc);
            bot.ThinkTimed(thinkTime);
        }
        else
        {
            // Assume infinite, think for however long you want!
            bot.ThinkInfinite();
        }
    }

    static void HandleStop()
    {
        bot.StopThinking();
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

    static void OnMoveChosen(string move)
    {
        Console.WriteLine($"bestmove {move}");
    }

    static int TryGetLabeledInt(string text, string label, int defaultValue = 0)
    {
        string valueString = TryGetLabeledValue(text, label);
        if (int.TryParse(valueString.Split(' ')[0], out int result))
            return result;

        return defaultValue;
    }

    static string TryGetLabeledValue(string text, string label, string defaultValue = "")
    {
        text = text.Trim();
        if (text.Contains(label))
        {
            int valueStart = text.IndexOf(label) + label.Length;
            int valueEnd = text.Length;
            return text.Substring(valueStart, valueEnd - valueStart).Trim();   
        }

        return defaultValue;
    }
}