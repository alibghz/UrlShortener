using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using UrlShortener.Services;
using UrlShortener.Services.ViewModels;
using UrlShortener.Web.Controllers.Api;
using Xunit;

namespace UrlShortener.Tests
{
    public class ShortenedUrlsControllerTests
    {
        private const string constUrl = "https://github.com/alibghz";
        private const string ranCod = "RanCod";

        [Fact]
        public void Get_ReturnsOk()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object);

            //act
            var action = controller.Get();

            //assert
            var result = Assert.IsType<OkObjectResult>(action);
            var value = Assert.IsAssignableFrom<string>(result.Value);
            Assert.Equal("Ok", value);
        }

        [Fact]
        public async Task GetCode_ReturnsUrlAsync()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetUrlByCode(It.IsAny<string>()))
                .ReturnsAsync(() => constUrl);
            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object);

            //act
            var action = await controller.Get(ranCod);

            //assert
            var result = Assert.IsType<OkObjectResult>(action);
            var value = Assert.IsAssignableFrom<string>(result.Value);
            Assert.Equal(constUrl, value);
        }

        [Fact]
        public async Task GetCode_ReturnsBadRequest_WhenCodeIsEmpty()
        {
            //arange
            var urlService = new Mock<IUrlService>();

            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object);

            //act
            var action = await controller.Get("");

            //assert
            Assert.IsType<BadRequestResult>(action);
        }

        [Fact]
        public async Task GetCode_ReturnsNotFound_WhenNoUrlProvidedByUrlService()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetUrlByCode(It.IsAny<string>()))
                .ReturnsAsync(() => "");

            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object);

            //act
            var action = await controller.Get(ranCod);

            //assert
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Post_CreatesShortUrl()
        {
            //arange
            var finalLink = $"https://url.sh/{ranCod}";
            var request = new Mock<HttpRequest>();
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetFinalLink(It.IsAny<HttpRequest>(), It.IsAny<string>()))
                .ReturnsAsync(() => finalLink);

            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object) { ControllerContext = controllerContext };
            var vm = new CreateShortUrlViewModel() { Url = constUrl };

            //act
            var action = await controller.Post(vm);

            //assert
            var result = Assert.IsType<CreatedResult>(action);
            var location = Assert.IsAssignableFrom<string>(result.Location);
            var value = Assert.IsAssignableFrom<string>(result.Value);
            Assert.Equal(finalLink, location);
            Assert.Equal(finalLink, value);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenModelStateIsNotValid()
        {
            //arange
            var urlService = new Mock<IUrlService>();

            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object);
            var vm = new CreateShortUrlViewModel() { Url = "" };
            controller.ModelState.AddModelError("Url", "Url is required");

            //act
            var action = await controller.Post(vm);

            //assert
            Assert.IsType<BadRequestResult>(action);
        }

        [Fact]
        public async Task Post_ReturnsBadRequestObject_WhenUrlServiceThrowsError()
        {
            //arange
            var urlService = new Mock<IUrlService>();
            urlService.Setup(x => x.GetFinalLink(It.IsAny<HttpRequest>(), It.IsAny<string>()))
                .ReturnsAsync(() => throw new Exception("Error"));

            var logger = new Mock<ILogger<ShortenedUrlsController>>();
            var controllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            var controller = new ShortenedUrlsController(logger.Object, urlService.Object) { ControllerContext = controllerContext };
            var vm = new CreateShortUrlViewModel() { Url = constUrl };

            //act
            var action = await controller.Post(vm);

            //assert
            var result = Assert.IsType<BadRequestObjectResult>(action);
            var value = Assert.IsAssignableFrom<string>(result.Value);
            Assert.Equal("Error", value);
        }
    }
}
