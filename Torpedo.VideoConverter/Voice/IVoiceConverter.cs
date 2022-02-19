using System.Threading.Tasks;

namespace Torpedo.Converters.Voice
{
    public interface IVoiceConverter
    {
        Task<string> ConvertAsync(string filePath);
    }
}