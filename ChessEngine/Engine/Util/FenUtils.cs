using System.Collections.ObjectModel;

public static class FenUtils
{
    public readonly struct PositionInfo
    {
        public readonly string fen;
        public readonly ReadOnlyCollection<int> squares;
        public readonly ulong[] bitboards;

        // Gamestate
        public readonly bool whiteCastleKingside;
        public readonly bool blackCastleKingside;
        public readonly bool whiteCastleQueenside;
        public readonly bool blackCastleQueenside;
        public readonly int enPassantSquare;
        public readonly bool whiteToMove;
        public readonly int fiftyMoveCounter;
        public readonly int moveCount;

        public PositionInfo(string fen)
        {
            this.fen = fen;
            int[] gameboard = new int[64];
            bitboards = new ulong[Piece.PieceIndexCount];

            string[] fenTokens = fen.Split(' ');

            // Decode the position from the FEN
            int file = 0, rank = 7;
            foreach (char symbol in fenTokens[0])
            {
                if (symbol == '/')
                {
                    file = 0;
                    rank--;
                }
                else
                {
                    if (char.IsDigit(symbol))
                    {
                        file += (int) char.GetNumericValue(symbol);
                    }
                    else
                    {
                        // Add piece to mailbox board
                        gameboard[rank * 8 + file] = Piece.FromChar(symbol);
                        // Add piece to bitboard array
                        bitboards[Piece.FromChar(symbol)] = Bitboard.SetSquare(bitboards[Piece.FromChar(symbol)], rank * 8 + file);
                        file++;
                    }
                }
            }
            squares = new(gameboard);

            // Gamestate
            whiteToMove = (fenTokens[1] == "w");

            string castlingRights = fenTokens[2];
            whiteCastleKingside = castlingRights.Contains('K');
            whiteCastleQueenside = castlingRights.Contains('Q');
            blackCastleKingside = castlingRights.Contains('k');
            blackCastleQueenside = castlingRights.Contains('q');

            enPassantSquare = 0;
            fiftyMoveCounter = 0;
            moveCount = 0;
            if (fenTokens.Length > 3)
            {
                enPassantSquare = fenTokens[3] != "-" ? BoardUtils.StringToSquare(fenTokens[3]) : 0;
            }

            if (fenTokens.Length > 4)
            {
                int.TryParse(fenTokens[4], out fiftyMoveCounter);
            }

            if (fenTokens.Length > 5)
            {
                int.TryParse(fenTokens[5], out moveCount);
            }
        }

    }

    public const string StartPosFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    /// <summary>
    /// Manipulates the <see cref="Board"/> to set up the gamestate and position of any given FEN string.
    /// </summary>
    /// <param name="board">The board to manipulate.</param>
    /// <param name="fen">The FEN to set the board up to.</param>
    public static PositionInfo SetupBoardFromFen(string fen = StartPosFen)
    {
        return new PositionInfo(fen);
    }
}