using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Torpedo.Converters
{
    public class XabeConverter : IVideoConverter
    {
        private static bool _ffmpegInitialized;

        // TODO: Input path to the FFMpeg exe files
        private static readonly string FFMPEG_EXECUTABLE_PATH ;

        static XabeConverter()
        {
            string UserSetting = Path.Combine(Directory.GetCurrentDirectory(), "UserSetting.json");
            string json;

            using (StreamReader r = new StreamReader(UserSetting))
            {
                json = r.ReadToEnd();
            }
            JToken jObj = JToken.Parse(json);

            FFMPEG_EXECUTABLE_PATH = (string)jObj.SelectToken("FFMPEG_EXECUTABLE_PATH");
        }
        public XabeConverter()
        {
            if (!_ffmpegInitialized)
            {
                FFmpeg.SetExecutablesPath(FFMPEG_EXECUTABLE_PATH);
                _ffmpegInitialized = true;
            }
        }

        public async Task<Stream> ConvertAsync(string filePath)
        {
            var outputPath = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
            var ms = new MemoryStream();

            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "watermark.png");

                IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                    ?.SetCodec(VideoCodec.h264)
                    ?.SetSize(VideoSize.Hvga)
                    ?.SetWatermark(watermarkPath, Position.Center);
                IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                    ?.SetCodec(AudioCodec.aac);

                var streams = new List<IStream>();

                if (videoStream != null) streams.Add(videoStream);
                if (audioStream != null) streams.Add(audioStream);

                if (!streams.Any()) throw new Exception("Video and Audio stream not found");

                await FFmpeg.Conversions.New()
                    .AddStream(streams)
                    .SetOutput(outputPath)
                    .Start();

                var fs = File.Open(outputPath, FileMode.Open);
                await fs.CopyToAsync(ms);
                fs.Close();

                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception)
            {
                await ms.DisposeAsync();
                throw;
            }
            finally
            {
                File.Delete(outputPath);
            }

            return ms;
        }
    }
}