using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot.Types;

namespace Torpedo.Bot.Utils
{
    public static class Notificator
    {
        private static readonly List<Action<string>> ActiveNotificationTypes = new List<Action<string>>
        {
            WriteToConsole,
            WriteToFile
        };

        public static void ConvertingReceiveStarted()
        {
            SendNotification("Converting receive is started!");
        }

        public static void ConvertingReceiveStopped()
        {
            SendNotification("Converting receive is stopped!");
        }

        public static void VideoSendSuccess(Message message, string caption)
        {
            SendNotification($"Send to chat: {message.Chat.Title}\n" +
                             $"Caption: {caption}\n" +
                             $"Replied to message: {message.MessageId}");
        }

        public static void VideoHandled(Message message)
        {
            SendNotification("Handled video\n" +
                             $"Chat: {message.Chat.Title}\n" +
                             $"FileSize: {message.Video.FileSize}\n" +
                             $"Description: {message.Caption}");
        }

        public static void VideoConverted(Message message, long resultLength)
        {
            SendNotification("Video converted!\n" +
                             $"Original File Size: {message.Video.FileSize}\n" +
                             $"Result File Size: {resultLength}");
        }

        public static void DirectTextMessage(Message message, string messageToClient)
        {
            SendNotification("New Direct Message!\n" +
                             $"From: {message.From.Username} ({message.GetFromFullName(false)})\n" +
                             $"Response: {messageToClient}\n" +
                             message.Text);
        }

        public static void Error(string message)
        {
            SendNotification(message);
        }

        #region Notification Type Settings

        private static void SendNotification(string message)
        {
            ActiveNotificationTypes.ForEach(t => t.Invoke(message));
        }

        private static void WriteToConsole(string message)
        {
            Console.WriteLine(Header + message);
        }

        private static void WriteToFile(string message)
        {
            using var file = new StreamWriter("messages_log.txt", true);
            file.WriteLine(Header + message);
        }

        private static string Header => $"\n------------------- {DateTime.Now:U} -------------------\n";

        #endregion
    }
}