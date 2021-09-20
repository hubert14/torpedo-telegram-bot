using System;
using System.Collections.Generic;
using System.Text;

namespace Torpedo.Infrastructure
{
   public
        class UserSetting
    {
        public UserSetting()
        {

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