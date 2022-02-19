using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Torpedo.Infrastructure;
using Xabe.FFmpeg;

namespace Torpedo.Converters.Video
{
    public class XabeConverter : IVideoConverter
    {
        public XabeConverter(Settings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.FFMpegPath))
            {
                FFmpeg.SetExecutablesPath(settings.FFMpegPath);
            }
        }

        public async Task<Stream> ConvertAsync(string filePath)
        {
            var tempFile = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempFile, ".mp4");

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

                Console.WriteLine("Start conversion. Output: " + outputPath);
                var result = await FFmpeg.Conversions.New()
                    .AddStream(streams)
                    .SetOutput(outputPath)
                    .Start();

                Console.WriteLine($"Conversion finished. Duration: {result.Duration}");

                using var fs = File.OpenRead(outputPath);
                await fs.CopyToAsync(ms);
                fs.Flush();

                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                Console.WriteLine("Convert error: " + e.Message);
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