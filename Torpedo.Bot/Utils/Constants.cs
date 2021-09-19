using Newtonsoft.Json.Linq;
using System.IO;

namespace Torpedo.Bot.Utils
{
    public class Constants
    {
        static Constants()
        {
            string UserSetting = Path.Combine(Directory.GetCurrentDirectory(), "UserSetting.json");
            string json;

            using (StreamReader r = new StreamReader(UserSetting))
            {
                json = r.ReadToEnd();
            }
            JToken jObj = JToken.Parse(json);
            jObj = jObj.SelectToken("Constans");

            RESULT_FILE_NAME =(string)jObj.SelectToken("RESULT_FILE_NAME");
            ERROR_UPLOAD_MESSAGE = (string)jObj.SelectToken("ERROR_UPLOAD_MESSAGE");
            WHY_MESSAGE = (string)jObj.SelectToken("WHY_MESSAGE");
            BOT_API_KEY = (string)jObj.SelectToken("BOT_API_KEY");
        }
        // TODO: Input your message for video title, which will be converted and send to the chat
        public static readonly string RESULT_FILE_NAME;

        // TODO: Input your message for error converting/uploading message
        public static readonly string ERROR_UPLOAD_MESSAGE;

        // TODO: Input your message for non-text content in the direct messages with bot
        public static readonly string WHY_MESSAGE;

        // TODO: Input your telegram bot API key
        public static readonly string BOT_API_KEY;
    }
}