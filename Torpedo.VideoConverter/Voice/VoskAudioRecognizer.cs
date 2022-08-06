using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Torpedo.Infrastructure;
using Vosk;
using Xabe.FFmpeg;

namespace Torpedo.Converters.Voice
{
    public class VoskAudioRecognizer : IVoiceConverter
    {
        private static Model VoskModel;
        private readonly Settings _settings;

        public VoskAudioRecognizer(Settings settings)
        {
            _settings = settings;

            try
            {
                if (!string.IsNullOrWhiteSpace(""))
                    FFmpeg.SetExecutablesPath("");

                var modelPath = _settings.VoskModelPath;

                Vosk.Vosk.SetLogLevel(-1); //Removes logs, put 0 to enable

                InitModel();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while init Vosk Audio");
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException?.Message);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        private static void InitModel()
        {
            VoskModel = new Model(Path.Combine(Directory.GetCurrentDirectory(), "Voice", "Model"));
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

            try
            {
                var rec = new VoskRecognizer(VoskModel, 16000.0f);

                rec.SetMaxAlternatives(0);

                rec.SetWords(true);

                await using (Stream source = File.OpenRead(outputPath))
                {
                    var buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        rec.AcceptWaveform(buffer, bytesRead);
                    }
                }

                var text = rec.FinalResult();

                return JObject.Parse(text).Last.First.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException?.Message);
                throw;
            }
            
        }
    }
}