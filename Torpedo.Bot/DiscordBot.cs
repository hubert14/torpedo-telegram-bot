using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Torpedo.Infrastructure;

namespace Torpedo.Bot
{
    public class DiscordBot
    {
        private readonly DiscordSettings _settings;
        private readonly DiscordSocketClient _discordClient;

        public DiscordBot(TelegramBot telegram, Settings settings)
        {
            _settings = settings.Discord;

            _discordClient = new DiscordSocketClient();

            _discordClient.Log += Log;
            _discordClient.MessageReceived += MessageReceived;
            _discordClient.UserJoined += UserJoined;

            _discordClient.LoginAsync(TokenType.Bot, _settings.Token).Wait();
            _discordClient.StartAsync().Wait();
            Console.WriteLine("Discord Bot init successfully.");
            telegram.FileUploaded += TelegramMessageReceived;
        }

        private async Task UserJoined(SocketGuildUser arg)
        {
            var guildId = arg.Guild.Id;
            if (guildId != _settings.GuildId) return;

            var mention = arg.Mention;
            
            var message = $"{mention} - {_settings.JoinMessage}";

            var a = arg.GetDefaultAvatarUrl();
            var cl = new HttpClient();
            var str = await cl.GetStreamAsync(a);

            var channel = arg.Guild.GetTextChannel(_settings.Channels.Main);
            await channel.SendFileAsync(str, "avatar.png", message);
        }

        public async Task TelegramMessageReceived(Stream fileStream, string fileName, string caption = null)
        {
            try
            {
                var guild = _discordClient.GetGuild(_settings.GuildId);
                var channel = guild.GetTextChannel(_settings.Channels.Memes);
                await channel.SendFileAsync(fileStream, fileName, caption);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content == "1")
            {
                await msg.Channel.SendMessageAsync(
                    text: "Заебал. Ну скажи уже блядь по человечески шо ты хочешь, Хлопчик",
                    messageReference: msg.Reference
                    );

                await msg.AddReactionAsync(Emoji.Parse("⚧"));
            }
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
