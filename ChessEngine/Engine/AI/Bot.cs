
public static class Bot
{
    static Move bestMove;



    public static void ThinkTimed(Board board, int timeMs)
    {
        bestMove = new();
        Search search = new Search();
        bestMove = search.StartSearch(board);
    }

    public static void StopThinking() 
    { 

    }

    public static Move BestMove()
    {
        return bestMove;
    }

}

