using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using jwtapi.Data;
using jwtapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jwtapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private ClientContext _client;
        public ClientController(ClientContext client)
        {
            _client = client;
        }

        [Authorize(Policy = "RequireManagerRole")]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddClient([FromBody]Client info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _client.AddAsync(info);
            await _client.SaveChangesAsync();
            return new OkObjectResult($"Added client {info.Name} {info.Surname} to database!");
        }

        [Authorize(Policy = "RequireManagerRole")]
        [HttpGet]
        [Route("List")]
        public IActionResult ListClient()
        {
            var clients = from c in _client.Client select c;
            return new OkObjectResult(clients);
        }
    }
}