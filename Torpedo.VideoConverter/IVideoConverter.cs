using System.IO;
using System.Threading.Tasks;

namespace Torpedo.VideoConverter
{
    public interface IVideoConverter
    {
        Task<Stream> ConvertAsync(string filePath);
    }
}