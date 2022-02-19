using System.IO;
using System.Threading.Tasks;

namespace Torpedo.Converters.Video
{
    public interface IVideoConverter
    {
        Task<Stream> ConvertAsync(string filePath);
    }
}