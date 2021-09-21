using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Torpedo.Infrastructure
{
   public
        class UserSetting
    {
        public UserSetting()
        {

        }
        public static UserSetting GetUserSettingFromFile()
        {
            string UserSetting = Path.Combine(Directory.GetCurrentDirectory(), "UserSetting.json");
            string json;

            using (StreamReader r = new StreamReader(UserSetting))
            {
                json = r.ReadToEnd();
            }
            return JsonSerializer.Deserialize<UserSetting>(json);
        }
        public string FFMPEG_EXECUTABLE_PATH { get; set; }

        public string RESULT_FILE_NAME { get; set; }
        public string ERROR_UPLOAD_MESSAGE { get; set; }
        public string WHY_MESSAGE { get; set; }
        public string BOT_API_KEY { get; set; }

        public string[] DirectPhrases { get; set; }
        public string[] CaptionPhrases { get; set; }
        public string[] VoicePhrases { get; set; }

        public string VOSK_RECOGNIZE_MODEL_PATH { get; set; }
    }
}