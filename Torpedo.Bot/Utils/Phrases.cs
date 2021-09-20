using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Torpedo.Infrastructure;

namespace Torpedo.Bot.Utils
{
    public static class Phrases
    {
        public static string RandomCaption => Content.GetRandomCaption();
        public static string RandomDirect => Content.GetRandomDirect();
        public static string RandomVoice => Content.GetRandomVoice();

        private static class Content
        {
            static Content()
            {
                string UserSetting = Path.Combine(Directory.GetCurrentDirectory(), "UserSetting.json");
                string json;

                using (StreamReader r = new StreamReader(UserSetting))
                {
                    json = r.ReadToEnd();
                }

                UserSetting userSetting = JsonSerializer.Deserialize<UserSetting>(json);

                Content.DirectPhrases = userSetting.DirectPhrases;

                Content.CaptionPhrases = userSetting.CaptionPhrases;

                Content.VoicePhrases = userSetting.VoicePhrases;


            }
            public static string GetRandomCaption()
            {
                var random = new Random();
                return CaptionPhrases[random.Next(0, CaptionPhrases.Length )];
            }

            public static string GetRandomDirect()
            {
                var random = new Random();
                return DirectPhrases[random.Next(0, DirectPhrases.Length )];
            }

            public static string GetRandomVoice()
            {
                var random = new Random();
                return VoicePhrases[random.Next(0, VoicePhrases.Length )];
            }

            private static readonly string[] DirectPhrases;

            private static readonly string[] CaptionPhrases;

            private static readonly string[] VoicePhrases;
        }
    }
}