using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace UrlShortener.Services
{
    public interface IUrlService
    {
        Task<string> GetCodeByUrl(string url);
        Task<string> GetRandomCode();
        Task<string> GetUrlByCode(string code);
        Task<string> GetFinalLink(HttpRequest request, string code);
        bool IsValidUrl(string url);
    }
}
