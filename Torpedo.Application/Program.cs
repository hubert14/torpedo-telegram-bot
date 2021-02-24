using System;
using Torpedo.Bot;

namespace Torpedo.Application
{
    internal class Program
    {
        private static BotApiClient _client;

        private static void Main(string[] args)
        {
            Console.WriteLine("Torpedo Club Features © 2021 Vitasya Tavern");
            Console.WriteLine("Press Q to stop telegram bot and close application");

            try
            {
                Console.WriteLine("Starting Telegram Bot");

                _client = new BotApiClient();
                var (id, name) = _client.GetBotInfoAsync().Result;

                Console.WriteLine($"Bot init successfully. ID: {id} | Name: {name}");

                _client.StartConvert();
            }
            catch (Exception e)
            {
                Console.WriteLine("Telegram Bot failed. Error:");
                Console.WriteLine(e.Message);

                _client.Dispose();
                return;
            }

            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);

            _client.Dispose();
        }
    }
}