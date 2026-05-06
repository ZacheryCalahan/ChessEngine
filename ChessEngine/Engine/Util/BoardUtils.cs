
public static class BoardUtils
{
    public static void PrintBoardChar(Board board)
    {
        Console.WriteLine(". a . b . c . d . e . f . g . h .");
        for (int i = 7; i >= 0; i--)
        {
            Console.WriteLine("|---|---|---|---|---|---|---|---|-");
            for (int j = 0; j < 8; j++)
            {
                Console.Write("| " + Piece.ToChar(board.GetPiece((i * 8) + j)) + " ");
            }
            Console.WriteLine("| " + (i + 1));
        }

        Console.WriteLine("|---|---|---|---|---|---|---|---|-");
    }

    public static void PrintBoardUnicode(Board board)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine(". a . b . c . d . e . f . g . h .");
        for (int i = 7; i >= 0; i--)
        {
            Console.WriteLine("|---|---|---|---|---|---|---|---|-");
            for (int j = 0; j < 8; j++)
            {
                string piece = Piece.ToUnicode(board.GetPiece((i * 8) + j));

                Console.Write($"| {piece} ");
            }
            Console.WriteLine("| " + (i + 1));
        }

        Console.WriteLine("|---|---|---|---|---|---|---|---|-");
    }

    public static void PrintAllBitboards(Board board)
    {
        Console.WriteLine($"White Pawns:    {board.GetBitboard(Piece.WhitePawn)}");
        Console.WriteLine($"White Rooks:    {board.GetBitboard(Piece.WhiteRook)}");
        Console.WriteLine($"White Bishops:  {board.GetBitboard(Piece.WhiteBishop)}");
        Console.WriteLine($"White Knights:  {board.GetBitboard(Piece.WhiteKnight)}");
        Console.WriteLine($"White Queens:   {board.GetBitboard(Piece.WhiteQueen)}");
        Console.WriteLine($"White King:     {board.GetBitboard(Piece.WhiteKing)}");

        Console.WriteLine($"Black Pawns:    {board.GetBitboard(Piece.BlackPawn)}");
        Console.WriteLine($"Black Rooks:    {board.GetBitboard(Piece.BlackRook)}");
        Console.WriteLine($"Black Bishops:  {board.GetBitboard(Piece.BlackBishop)}");
        Console.WriteLine($"Black Knights:  {board.GetBitboard(Piece.BlackKnight)}");
        Console.WriteLine($"Black Queens:   {board.GetBitboard(Piece.BlackQueen)}");
        Console.WriteLine($"Black King:     {board.GetBitboard(Piece.BlackKing)}");

        Console.WriteLine($"White Pieces:   {board.GetWhiteBitboard()}");
        Console.WriteLine($"Black Pieces:   {board.GetBlackBitboard()}");
    }

    public static void PrintAllMoves(Board board)
    {
        List<Move> moves = MoveGenerator.GenerateLegalMoves(board);
        foreach (Move move in moves)
        {
            Console.WriteLine(move.ToString());
        }
    }

    public static string SquareToString(int coord)
    {

        string ranks = "abcdefgh";

        int rankIdx = coord % 8;
        string coordName = "" + ranks[rankIdx]; // this is a dumb fix.

        int fileNum = (coord / 8) + 1;
        coordName += fileNum;

        return coordName;

    }

    public static int StringToSquare(string s)
    {
        if (s.Length != 2)
            throw new ArgumentException("Invalid square string.");

        int fileNum = s[0] - 'a';
        int rankNum = s[1] - '1';

        if (fileNum < 0 || fileNum > 7 || rankNum < 0 || rankNum > 7)
            throw new ArgumentException("Invalid square string.");

        return rankNum * 8 + fileNum;
    }

    public static int FileIndex(int square)
    {
        return square >> 3;
    }

    public static int RankIndex(int square)
    {
        return square & 0b111;
    }

}
