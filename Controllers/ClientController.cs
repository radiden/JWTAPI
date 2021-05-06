using System.Linq;
using System.Threading.Tasks;
using jwtapi.Data;
using jwtapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var response = new OkResponse
            {
                Info = "Added successfully!",
                Data = $"Client: {info.Name} {info.Surname}"
            };
            return new OkObjectResult(response);
        }

        [Authorize(Policy = "RequireManagerRole")]
        [HttpDelete]
        [Route("Remove")]
        public async Task<IActionResult> RemoveClient(int id)
        {
            var client = await _client.Client.FirstOrDefaultAsync(r => r.Id == id);
            _client.Remove(client);
            await _client.SaveChangesAsync();
            var response = new OkResponse
            {
                Info = "Removed successfully!",
                Data = $"Client: {client.Name} {client.Surname}"
            };
            return new OkObjectResult(response);
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