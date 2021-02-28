using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Torpedo.Bot.Utils;
using Torpedo.VideoConverter;
using File = System.IO.File;

namespace Torpedo.Bot
{
    public class BotApiClient : IDisposable
    {
        private readonly TelegramBotClient _client;
        private readonly IVideoConverter _converter;

        public BotApiClient()
        {
            _client = new TelegramBotClient(Constants.BOT_API_KEY);
            _converter = new XabeConverter();
        }

        public async Task<(int Id, string Name)> GetBotInfoAsync()
        {
            var me = await _client.GetMeAsync();
            return (me.Id, me.FirstName);
        }

        public void StartConvert()
        {
            if (_client.IsReceiving) return;

            _client.OnMessage += HandleMessage;
            _client.StartReceiving();

            Notificator.ConvertingReceiveStarted();
        }

        public void StopConvert()
        {
            if (!_client.IsReceiving) return;

            _client.OnMessage -= HandleMessage;
            _client.StopReceiving();

            Notificator.ConvertingReceiveStopped();
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
            {
                ProcessDirectMessage(e.Message);
                return;
            }

            if (e.Message.ForwardFrom != null) return;

            switch (e.Message.Type)
            {
                case MessageType.Video:
                    ProcessConvert(e.Message);
                    break;
                case MessageType.Voice:
                    ProcessVoice(e.Message);
                    break;
            }
        }

        private void ProcessDirectMessage(Message message)
        {
            var messageToClient = message.Type == MessageType.Text
                ? Phrases.RandomDirect
                : Constants.WHY_MESSAGE;

            Notificator.DirectTextMessage(message, messageToClient);

            Task.Run(async () => { await _client.SendTextMessageAsync(message.Chat.Id, messageToClient); });
        }

        private void ProcessConvert(Message message)
        {
            Task.Run(async () =>
            {
                var fileName = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
                var fs = File.Create(fileName);

                Stream videoStream = null;

                try
                {
                    Notificator.VideoHandled(message);

                    var file = await _client.GetFileAsync(message.Video.FileId);

                    await _client.DownloadFileAsync(file.FilePath, fs);
                    fs.Close();

                    videoStream = await _converter.ConvertAsync(fs.Name);

                    Notificator.VideoConverted(message, videoStream.Length);

                    var caption = message.GetFromFirstName() + Phrases.RandomCaption;

                    var inputFile = new InputOnlineFile(videoStream, Constants.RESULT_FILE_NAME);
                    await _client.SendVideoAsync(message.Chat.Id, inputFile,
                        caption: caption,
                        replyToMessageId: message.MessageId);

                    Notificator.VideoSendSuccess(message, caption);
                }
                catch (Exception exc)
                {
                    Notificator.Error(exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        message.GetFromFirstName() + Constants.ERROR_UPLOAD_MESSAGE,
                        replyToMessageId: message.MessageId);
                }
                finally
                {
                    File.Delete(fs.Name);
                    videoStream?.Dispose();
                }
            });
        }

        private void ProcessVoice(Message message)
        {
            Task.Run(async () => { await _client.SendTextMessageAsync(message.Chat.Id, Phrases.RandomVoice, replyToMessageId: message.MessageId); });
        }

        public void Dispose()
        {
            StopConvert();
        }
    }
}