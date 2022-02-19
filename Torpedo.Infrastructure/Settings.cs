namespace Torpedo.Infrastructure
{
    public class Settings
    {
        public TelegramSettings Telegram { get; set; }
        public DiscordSettings Discord { get; set; }

        public string FFMpegPath { get; set; }
        
        public string VoskModelPath { get; set; }
    }

    public class TelegramSettings
    {
        public string Token { get; set; }

        public string ResultFileName { get; set; }

        public TelegramMessages Messages { get; set; }

    }

    public class DiscordSettings
    {
        public string Token { get; set; }

        public string JoinMessage { get; set; }
        
        public ulong GuildId { get; set; }
        public DiscordChannels Channels { get; set; }
    }

    public class TelegramMessages
    {
        public string ErrorUploadMessage { get; set; }
        public string NonTextDirectMessage { get; set; }

        public string[] DirectPhrases { get; set; }
        public string[] CaptionPhrases { get; set; }
        public string[] VoicePhrases { get; set; }
    }

    public class DiscordChannels
    {
        public ulong Memes { get; set; }
        public ulong Main { get; set; }
    }
}