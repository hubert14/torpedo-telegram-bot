using System;

namespace Torpedo.Bot.Utils
{
    public static class ArrayExtensions
    {
        public static string GetRandom(this string[] collection)
        {
            var random = new Random();
            return collection[random.Next(0, collection.Length)];
        }
    }
}