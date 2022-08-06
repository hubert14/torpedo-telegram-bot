using System.IO;
using System.Threading.Tasks;

namespace Torpedo.Converters.Video
{
    public interface IVideoConverter
    {
        Task<MemoryStream> ConvertAsync(string filePath);
    }
}