using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using UrlShortener.Domain;
using UrlShortener.Domain.Entities;
using UrlShortener.Services;
using Xunit;

namespace UrlShortener.Tests
{
    public class UrlServiceTests
    {
        private readonly Mock<ILogger<UrlService>> logger;
        private readonly Mock<IOptions<UrlServiceOptions>> urlServiceOption;

        public UrlServiceTests()
        {
            logger = new Mock<ILogger<UrlService>>();
            urlServiceOption = new Mock<IOptions<UrlServiceOptions>>();
            urlServiceOption.Setup(x => x.Value)
                .Returns(new UrlServiceOptions { CodeLength = 6 });
        }

        [Fact]
        public async Task GetRandomCode_CanGenerate6CharsCodeAsync()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act
                var code = await urlService.GetRandomCode();

                //assert
                Assert.Equal(urlServiceOption.Object.Value.CodeLength, code.Length);
            }
        }

        [Fact]
        public void IsValidUrl_ValidatesHttpAndHttpsUrls()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act
                var validHttpUrl = "http://google.com";
                var validHttpsUrl = "https://google.com";
                var invalidUrl = "google.com";

                //assert
                Assert.True(urlService.IsValidUrl(validHttpUrl));
                Assert.True(urlService.IsValidUrl(validHttpsUrl));
                Assert.False(urlService.IsValidUrl(invalidUrl));
            }
        }

        [Fact]
        public async Task GetUrlByCode_ReturnsActualUrlAsync()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act
                var entity = new ShortenedUrl()
                {
                    Id = Guid.NewGuid(),
                    Code = await urlService.GetRandomCode(),
                    Url = "https://github.com/alibghz",
                    CreatedAtUtc = DateTime.UtcNow,
                };
                context.Add(entity);
                context.SaveChanges();
                var url = await urlService.GetUrlByCode(entity.Code);

                //assert
                Assert.True(url == entity.Url);
            }
        }

        [Fact]
        public async Task GetCodeByUrl_CreateNewShortenedUrlWithProperCodeAsync()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act
                var url = "https://github.com/alibghz";
                var code = await urlService.GetCodeByUrl(url);

                var entity = await context.ShortenedUrls.FirstOrDefaultAsync(x => x.Url == url);

                //assert
                Assert.NotNull(entity);
                Assert.True(entity.Code == code);
            }
        }

        [Fact]
        public async Task GetCodeByUrl_RejectsEmptyUrl()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act - assert
                Exception ex = await Assert.ThrowsAsync<Exception>(() =>
                                    //act
                                    urlService.GetCodeByUrl("")
                                );

                //assert
                Assert.Equal("Empty Url", ex.Message);
            }
        }

        [Fact]
        public async Task GetCodeByUrl_RejectsInvalidUrl()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act - assert
                Exception ex = await Assert.ThrowsAsync<Exception>(() =>
                                    //act
                                    urlService.GetCodeByUrl("//github.com/alibghz")
                                );

                //assert
                Assert.Equal("Invalid Url", ex.Message);
            }
        }

        [Fact]
        public async Task GetCodeByUrl_ReturnsSameCodeForSameUrl()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                //act
                var url = "https://github.com/alibghz";
                var code1 = await urlService.GetCodeByUrl(url);
                var code2 = await urlService.GetCodeByUrl(url);
                var count = await context.ShortenedUrls.CountAsync();

                //assert
                Assert.Equal(1, count);
                Assert.Equal(code1, code2);
            }
        }

        [Fact]
        public async Task GetFinalLink_ReturnsUrlUsingHttpRequest()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);

                var request1 = new Mock<HttpRequest>();
                request1.Setup(x => x.Scheme).Returns("https");
                request1.Setup(x => x.Host).Returns(new HostString("te.st", 444));

                var request2 = new Mock<HttpRequest>();
                request2.Setup(x => x.Scheme).Returns("https");
                request2.Setup(x => x.Host).Returns(new HostString("te.st", 443));

                var request3 = new Mock<HttpRequest>();
                request3.Setup(x => x.Scheme).Returns("http");
                request3.Setup(x => x.Host).Returns(new HostString("te.st", 81));

                var request4 = new Mock<HttpRequest>();
                request4.Setup(x => x.Scheme).Returns("http");
                request4.Setup(x => x.Host).Returns(new HostString("te.st", 80));

                //act
                var url = "https://github.com/alibghz";

                var finalLink1 = await urlService.GetFinalLink(request1.Object, url);
                var finalLink2 = await urlService.GetFinalLink(request2.Object, url);
                var finalLink3 = await urlService.GetFinalLink(request3.Object, url);
                var finalLink4 = await urlService.GetFinalLink(request4.Object, url);

                //assert
                Assert.StartsWith("https://te.st:444/", finalLink1);
                Assert.StartsWith("https://te.st/", finalLink2);
                Assert.StartsWith("http://te.st:81/", finalLink3);
                Assert.StartsWith("http://te.st/", finalLink4);
            }
        }

        [Fact]
        public async Task GetFinalLink_ReturnsUrlUsingBaseUrlOption()
        {
            //arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseInMemoryDatabase($"Db{Guid.NewGuid()}")
                                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                urlServiceOption.Setup(x => x.Value)
                    .Returns(new UrlServiceOptions { CodeLength = 6, BaseUrl = "http://te.st/" });

                var urlService = new UrlService(urlServiceOption.Object, logger.Object, context);
                var request = new Mock<HttpRequest>();

                //act
                var url = "https://github.com/alibghz";

                var finalLink1 = await urlService.GetFinalLink(request.Object, url);

                //assert
                Assert.StartsWith("http://te.st/", finalLink1);
            }
        }
    }
}
