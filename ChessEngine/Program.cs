class Program
{
    public static void Main(string[] args)
    {
        string command = Console.ReadLine();
        if (command == "uci")
        {
            UCI.Start();
            return;
        }

        //CLI.Start(); // Not implemented.
    }
}