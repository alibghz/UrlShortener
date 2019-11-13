using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UrlShortener.Services;
using UrlShortener.Services.ViewModels;

namespace UrlShortener.Web.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShortenedUrlsController : ControllerBase
    {
        private readonly ILogger<ShortenedUrlsController> logger;
        private readonly IUrlService urlService;

        public ShortenedUrlsController(
            ILogger<ShortenedUrlsController> logger,
                IUrlService urlService
            )
        {
            this.urlService = urlService;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok("Ok");
        }

        /// <response code="200">Returns mapped url</response>
        /// <response code="400">If the code is null</response>
        /// <response code="404">If there is no corresponding url</response>  
        [HttpGet("{code}", Name = "Get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(CacheProfileName = "DefaultResponseCache")]
        public async Task<ActionResult> Get(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest();

            var url = await urlService.GetUrlByCode(code);

            if (string.IsNullOrWhiteSpace(url))
                return NotFound();
            return Ok(url);
        }

        /// <response code="201">Create new ShortenedUrl</response>
        /// <response code="404">If url is not valid or there is a problem creating new record</response> 
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] CreateShortUrlViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string url = await urlService.GetFinalLink(Request, vm.Url);
                    return Created(url, url);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }
    }
}
