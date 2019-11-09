using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UrlShortener.Services;
using UrlShortener.Web;
using Xunit;

namespace UrlShortener.Tests
{

    public class StartupTests
    {
        [Fact]
        public void WebHost_HasUrlService()
        {
            var webHost = Program.CreateHostBuilder(new string[] { }).Build();

            Assert.NotNull(webHost);
            Assert.NotNull(webHost.Services.GetRequiredService<IUrlService>());
        }
    }
}
