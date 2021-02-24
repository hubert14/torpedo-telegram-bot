using System.Text;
using Telegram.Bot.Types;

namespace Torpedo.Bot.Utils
{
    public static class TelegramMessageExtensions
    {
        public static string GetFromFullName(this Message message, bool withComma = true)
        {
            var from = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(message.From.FirstName))
            {
                from.Append(message.From.FirstName);
            }

            if (!string.IsNullOrWhiteSpace(message.From.LastName))
            {
                from.Append(" ");
                from.Append(message.From.LastName);
            }

            if (withComma && from.Length > 0) from.Append(", ");

            return from.ToString();
        }

        public static string GetFromFirstName(this Message message, bool withComma = true)
        {
            if (string.IsNullOrWhiteSpace(message.From.FirstName)) return string.Empty;

            return withComma ? message.From.FirstName + ", " : message.From.FirstName;
        }
    }
}