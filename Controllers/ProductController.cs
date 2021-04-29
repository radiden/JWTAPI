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
    public class ProductController : ControllerBase
    {
        private ProductContext _product;
        public ProductController(ProductContext product)
        {
            _product = product;
        }

        [Authorize(Policy = "RequireManagerRole")]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddProduct([FromBody]Product info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _product.AddAsync(info);
            await _product.SaveChangesAsync();
            return new OkObjectResult($"Added product {info.Name} to database!");
        }

        [Authorize(Policy = "RequireCustomerRole")]
        [HttpGet]
        [Route("List")]
        public IActionResult ListProduct()
        {
            var products = from p in _product.Product select p;
            return new OkObjectResult(products);
        }
    }
}