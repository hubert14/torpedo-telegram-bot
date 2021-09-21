using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vosk;
using Xabe.FFmpeg;
using Torpedo.Infrastructure;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Torpedo.Converters
{
    public class VoskAudioRecognizer : IVoiceConverter
    {
        private static readonly string FFMPEG_EXECUTABLE_PATH;
        private static readonly Model VoskModel;

        static VoskAudioRecognizer()
        {
            UserSetting userSetting = UserSetting.GetUserSettingFromFile();

            FFMPEG_EXECUTABLE_PATH = userSetting.FFMPEG_EXECUTABLE_PATH;

            FFmpeg.SetExecutablesPath(FFMPEG_EXECUTABLE_PATH);

            string VoskRecognizeModelPath= userSetting.VOSK_RECOGNIZE_MODEL_PATH;
            
            Vosk.Vosk.SetLogLevel(-1);//Removes logs, put 0 to enable
            
            VoskModel = new Model(VoskRecognizeModelPath);

        }
        public async Task<string> ConvertAsync(string filePath)
        {

            var mediaInfo = FFmpeg.GetMediaInfo(filePath).Result;


            IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
               ?.SetCodec(AudioCodec.pcm_s16le)
               ?.SetChannels(1)
               ?.SetSampleRate(16000);

            var outputPath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");

             await FFmpeg.Conversions.New().AddStream(audioStream).SetOutput(outputPath).Start();


            VoskRecognizer rec = new VoskRecognizer(VoskAudioRecognizer.VoskModel, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);

            using (Stream source = File.OpenRead(outputPath))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    rec.AcceptWaveform(buffer, bytesRead);
                }
            }
            string text = rec.FinalResult();

            ;
            return JObject.Parse(text).Last.First.ToString();
        }
    }
}