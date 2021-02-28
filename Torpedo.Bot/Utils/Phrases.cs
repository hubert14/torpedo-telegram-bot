using System;

namespace Torpedo.Bot.Utils
{
    public static class Phrases
    {
        public static string RandomCaption => Content.GetRandomCaption();
        public static string RandomDirect => Content.GetRandomDirect();
        public static string RandomVoice => Content.GetRandomVoice();

        private static class Content
        {
            public static string GetRandomCaption()
            {
                var random = new Random();
                return CaptionPhrases[random.Next(0, CaptionPhrases.Length - 1)];
            }

            public static string GetRandomDirect()
            {
                var random = new Random();
                return DirectPhrases[random.Next(0, DirectPhrases.Length - 1)];
            }

            public static string GetRandomVoice()
            {
                var random = new Random();
                return VoicePhrases[random.Next(0, VoicePhrases.Length - 1)];
            }

            private static readonly string[] DirectPhrases =
            {
                // TODO: Input phrases, which will be send to the client in direct chat with bot
            };

            private static readonly string[] CaptionPhrases =
            {
                // TODO: Input phrases, which will be send in the video caption with converted video
            };

            private static readonly string[] VoicePhrases =
            {
                // TODO: Input phrases, which will be send in the video caption with converted video
            };
        }
    }
}