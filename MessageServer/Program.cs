using System;

namespace MessageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int failCounter = 0;
            Console.WriteLine("Message Server Starting");
            Console.Write("Starting IRC Server...");
            try
            {
                IRCEngine irc = new IRCEngine();
                irc.Run();
                Console.WriteLine("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed: " + e.Message);
                failCounter++;
            }

            if (failCounter == 0)
                Console.WriteLine("Message Server Ready");
            else
                Console.WriteLine("Message Server Failed to Start");
        }
    }
}
