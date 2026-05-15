class Program
{
    public static void Main(string[] args)
    {
        string command = Console.ReadLine();
        if (command == "uci")
            UCI.Start();

        //CLI.Start(); // Not implemented.

        // Test relevancy masks.
        for (int i = 0; i < 64; i++)
        {
            ulong attack = MagicBitboard.CreateMovementMaskOrtho(i, 0, true);
            Console.WriteLine(Bitboard.ToStringMarker(attack, i));
            Console.ReadLine();
        }
    }
}