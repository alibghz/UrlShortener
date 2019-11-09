using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UrlShortener.Services;

namespace UrlShortener.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ResponseCache(CacheProfileName = "DefaultResponseCache")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IUrlService urlService;

        public HomeController(ILogger<HomeController> logger, IUrlService urlService)
        {
            this.logger = logger;
            this.urlService = urlService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("{code}")]
        public async Task<IActionResult> OpenUrl(string code)
        {
            var url = await urlService.GetUrlByCode(code);
            if (string.IsNullOrWhiteSpace(url)) return NotFound();
            return Redirect(url);
        }

        [Route("ping")]
        [ResponseCache(NoStore = true)]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
