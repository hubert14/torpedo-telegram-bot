using System.IO;
using System.Threading.Tasks;

namespace Torpedo.Converters
{
    public interface IVideoConverter
    {
        Task<Stream> ConvertAsync(string filePath);
    }
}