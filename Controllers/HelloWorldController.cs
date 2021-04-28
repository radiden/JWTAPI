using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using jwtapi.Models;

namespace jwtapi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class HelloWorldController : ControllerBase
    {
        private readonly ILogger<HelloWorldController> _logger;

        public HelloWorldController(ILogger<HelloWorldController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation($"Got request! IsAuthenticated: {User.Identity.IsAuthenticated.ToString()} with {User.Identity.AuthenticationType}");
            if (User.Identity.IsAuthenticated) {
                return new OkObjectResult(new HelloWorld{Hello = "hello, world!"});
            }
            else {
                return new UnauthorizedResult();
            }
        }
        
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]
        [Route("ServerInfo")]
        public IActionResult Info()
        {
            _logger.LogInformation($"Got request! IsAuthenticated: {User.Identity.IsAuthenticated.ToString()} with {User.Identity.AuthenticationType}");
            if (User.Identity.IsAuthenticated) {
                return new OkObjectResult(new {Info = $"{RuntimeInformation.OSDescription}"});
            }
            else {
                return new UnauthorizedResult();
            }
        }
    }
}
