using System.Threading.Tasks;

namespace Torpedo.Converters
{
    public interface IVoiceConverter
    {
        Task<string> ConvertAsync(string filePath);
    }
}