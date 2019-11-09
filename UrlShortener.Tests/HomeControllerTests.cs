using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using UrlShortener.Services;
using UrlShortener.Web.Controllers;
using Xunit;

namespace UrlShortener.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(logger.Object, urlService.Object);

            //act
            var action = controller.Index();

            //assert
            Assert.IsType<ViewResult>(action);
        }

        [Fact]
        public void Ping_Pong()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(logger.Object, urlService.Object);

            //act
            var action = controller.Ping();

            //assert
            var result = Assert.IsType<OkObjectResult>(action);
            var value = Assert.IsAssignableFrom<string>(result.Value);
            Assert.Equal("pong", value);
        }

        [Fact]
        public async Task OpenUrl_RedirectsToUrl()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetUrlByCode(It.IsAny<string>()))
                .ReturnsAsync(() => "https://github.com/alibghz");

            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(logger.Object, urlService.Object);

            //act
            var action = await controller.OpenUrl("RanCod");

            //assert
            var result = Assert.IsType<RedirectResult>(action);
            var url = Assert.IsAssignableFrom<string>(result.Url);
            Assert.Equal("https://github.com/alibghz", url);
        }

        [Fact]
        public async Task OpenUrl_ReturnsNotFound_WhenNoUrlProvidedByUrlService()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetUrlByCode(It.IsAny<string>()))
                .ReturnsAsync(() => "");

            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(logger.Object, urlService.Object);

            //act
            var action = await controller.OpenUrl("RanCod");

            //assert
            Assert.IsType<NotFoundResult>(action);
        }
    }
}
