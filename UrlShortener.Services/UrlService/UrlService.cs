using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Domain;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Services
{

    public class UrlService : IUrlService
    {
        private readonly UrlServiceOptions options;
        private readonly ILogger<UrlService> logger;
        private readonly ApplicationDbContext context;
        private readonly string chars = "abcdefghijklmnopqrstuvwzyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static Random random = new Random();

        public UrlService(
                IOptions<UrlServiceOptions> optionAccessor,
                ILogger<UrlService> logger,
                ApplicationDbContext context
            )
        {
            this.options = optionAccessor.Value;
            this.logger = logger;
            this.context = context;
        }

        public async Task<string> GetUrlByCode(string code)
        {
            return await context.ShortenedUrls
                .Where(x => x.Code == code)
                .Select(x => x.Url)
                .AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<string> GetCodeByUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Empty Url");

            if (!IsValidUrl(url))
                throw new Exception("Invalid Url");

            var code = await context.ShortenedUrls
                .Where(x => x.Url == url)
                .Select(x => x.Code)
                .AsNoTracking().FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(code))
            {
                code = await GetRandomCode();
                var entity = new ShortenedUrl() { Id = Guid.NewGuid(), Url = url, Code = code };
                context.Add(entity);
                await context.SaveChangesAsync();
            }
            return code;
        }

        public async Task<string> GetRandomCode()
        {
            var code = new string(
                        Enumerable.Repeat(chars, options.CodeLength)
                            .Select(s => s[random.Next(s.Length)])
                            .ToArray()
                    );

            //check if the code already exist in db
            if (await context.ShortenedUrls.AnyAsync(x => x.Code == code))
            {
                logger.LogWarning($"Duplicated code generated ({code})");
                return await GetRandomCode();
            }
            return code;
        }

        public bool IsValidUrl(string url)
        {
            return url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public async Task<string> GetFinalLink(HttpRequest request, string url)
        {
            var code = await GetCodeByUrl(url);
            var port = request.Scheme.ToLower() == "http" ? (request.Host.Port != 80 ? $":{request.Host.Port}" : "") : (request.Host.Port != 443 ? $":{request.Host.Port}" : "");
            port = port.TrimEnd(new char[] { ':' });
            var host = request.Host.Host.TrimEnd(new char[] { ':', '/' });
            return $"{request.Scheme}://{host}{port}/{code}";
        }
    }
}
