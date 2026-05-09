
using System.Collections;

public class PieceList : IEnumerable<int>
{
    int pieceCount = 0;
    public int[] occupiedSquares;
    int[] map;

    public PieceList()
    {
        occupiedSquares = new int[16];
        map = new int[64];
        pieceCount = 0;
    }

    public int Count => pieceCount;

    public void AddPiece(int square)
    {
        occupiedSquares[pieceCount] = square;
        map[square] = pieceCount;
        pieceCount++;
    }

    public void RemovePiece(int square)
    {
        int pieceIndex = map[square];
        occupiedSquares[pieceIndex] = occupiedSquares[pieceCount - 1];
        map[occupiedSquares[pieceIndex]] = pieceIndex;
        pieceCount--;
    }

    public void MovePiece(int startSquare, int targetSquare)
    {
        int pieceIndex = map[startSquare];
        occupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < pieceCount; i++)
        {
            yield return occupiedSquares[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return occupiedSquares.GetEnumerator();
    }

    public int this[int index] => occupiedSquares[index];
}

