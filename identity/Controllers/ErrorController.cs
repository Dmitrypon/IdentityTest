using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Palantir.Identity.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        [HttpGet]
        public IActionResult ProcessErrors()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            _logger.LogError(context.Error, "Unhandled error");
            return Problem(
            detail: context.Error.StackTrace,
            title: context.Error.Message);
        }
    }
}