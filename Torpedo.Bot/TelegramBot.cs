using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Torpedo.Bot.Utils;
using Torpedo.Converters.Video;
using Torpedo.Converters.Voice;
using Torpedo.Infrastructure;
using Xabe.FFmpeg;
using File = System.IO.File;

namespace Torpedo.Bot
{
    public delegate Task NewContentHandler(Stream fileStream, string caption = null);

    public sealed class TelegramBot : IDisposable
    {
        private readonly TelegramBotClient _client;
        private readonly IVideoConverter _videoСonverter;
        private readonly IVoiceConverter _voiceСonverter;
        private readonly TelegramSettings _settings;
        public event NewContentHandler FileUploaded;

        public TelegramBot(Settings settings, XabeConverter xabeConverter)
        {
            _settings = settings.Telegram;
            _videoСonverter = xabeConverter;
            //_voiceСonverter = new VoskAudioRecognizer();

            Console.WriteLine("Starting Telegram Bot");

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            _client = new TelegramBotClient(_settings.Token);
            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            var (id, name) = GetBotInfoAsync().Result;

            Console.WriteLine($"Telegram Bot init successfully. ID: {id} | Name: {name}");
        }

        public async Task<(long Id, string Name)> GetBotInfoAsync()
        {
            var me = await _client.GetMeAsync();
            return (me.Id, me.FirstName);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                switch (message.Chat.Type)
                {
                    case ChatType.Private:
                        await ProcessDirectMessageAsync(message);
                        return;
                }

                if (message.ForwardFrom != null) return;

                switch (message.Type)
                {
                    case MessageType.Video:
                        await ProcessConvertAsync(message);
                        break;
                        //case MessageType.Voice:
                        //    ProcessVoice(e.Message);
                        //    break;
                }
            }
            else if (update.ChannelPost is Message channelPost)
            {
                await ProcessChannelMessageAsync(channelPost);
            }
        }

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                Console.WriteLine(apiRequestException.Message);
            }
        }

        private async Task ProcessChannelMessageAsync(Message message)
        {
            if (message.Type != MessageType.Photo) return;

            var maxResolutionPhoto = message.Photo.First(x => x.FileSize == message.Photo.Max(x => x.FileSize));

            var ms = new MemoryStream();
            var stream = await _client.GetInfoAndDownloadFileAsync(maxResolutionPhoto.FileId, ms);

            await Task.Delay(5000);
            FileUploaded?.Invoke(ms, message.Caption);
        }

        private async Task ProcessDirectMessageAsync(Message message)
        {
            var messageToClient = message.Type == MessageType.Text
                ? _settings.Messages.DirectPhrases.GetRandom()
                : _settings.Messages.NonTextDirectMessage;

            Notificator.DirectTextMessage(message, messageToClient);

            await _client.SendTextMessageAsync(message.Chat.Id, messageToClient);
        }

        private async Task ProcessConvertAsync(Message message)
        {
            try
            {
                var tempFile = Path.GetTempFileName();
                var fileName = Path.ChangeExtension(tempFile, ".mp4");
                using var file = File.Create(fileName);

                try
                {
                    Notificator.VideoHandled(message);

                    var video = await _client.GetFileAsync(message.Video.FileId);
                    await _client.DownloadFileAsync(video.FilePath, file);

                    file.Flush();
                    file.Close();

                    using var videoStream = await _videoСonverter.ConvertAsync(fileName);

                    Notificator.VideoConverted(message, videoStream.Length);

                    var caption = message.GetFromFirstName() + _settings.Messages.CaptionPhrases.GetRandom();

                    var inputFile = new InputOnlineFile(videoStream, _settings.ResultFileName);
                    await _client.SendVideoAsync(message.Chat.Id, inputFile,
                        caption: caption,
                        replyToMessageId: message.MessageId);

                    Notificator.VideoSendSuccess(message, caption);
                }
                catch (Exception exc)
                {
                    Notificator.Error("Exception: " + exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"{message.GetFromFirstName()},\n" +
                        $"{exc.Message}\n" +
                        $"{exc.StackTrace}",
                        replyToMessageId: message.MessageId);
                }
                finally
                {
                    File.Delete(file.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ProcessVoice(Message message)
        {
            Task.Run(async () =>
            {
                var fileName = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
                var fs = File.Create(fileName);

                try
                {
                    Notificator.VoiceHandled(message);

                    var file = await _client.GetFileAsync(message.Voice.FileId);
                    await _client.DownloadFileAsync(file.FilePath, fs);
                    fs.Close();

                    var mediaInfo = FFmpeg.GetMediaInfo(fileName).Result;

                    IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                        ?.SetCodec(AudioCodec.pcm_s16le)
                        ?.SetChannels(1)
                        ?.SetSampleRate(16000);

                    var outputPath = @"C:\test\.test.wav"; //Path.ChangeExtension(Path.GetTempFileName(), ".wav");

                    await FFmpeg.Conversions.New().AddStream(audioStream).SetOutput(outputPath).Start();

                    //var text = await _voiceСonverter.ConvertAsync(fs.Name);
                    //Notificator.VoiceConverted(message, text.Length);

                    //await _client.SendTextMessageAsync(message.Chat.Id, PhrasesHelper.RandomVoice + $"\n{text}.", replyToMessageId: message.MessageId);
                }
                catch (Exception exc)
                {
                    Notificator.Error(exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        message.GetFromFirstName() + _settings.Messages.ErrorUploadMessage,
                        replyToMessageId: message.MessageId);
                }
                finally
                {
                    File.Delete(fs.Name);
                }
            });
        }

        public void Dispose()
        {
            _client.CloseAsync();
        }
    }
}