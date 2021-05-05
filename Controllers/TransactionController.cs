using System;
using System.Data.Entity;
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
    public class TransactionController : ControllerBase
    {
        private TransactionContext _transaction;
        private ClientContext _client;
        private ProductContext _product;
        public TransactionController(TransactionContext transaction, ClientContext client, ProductContext product)
        {
            _transaction = transaction;
            _client = client;
            _product = product;
        }

        [Authorize(Policy = "RequireManagerRole")]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddTransaction([FromBody]TransactionPost info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (info.ClientId == 0 || info.ProductId == 0)
            {
                return new BadRequestObjectResult("Client/Product ID can't be 0!");
            }
            
            var transaction = new TransactionProd
            {
                // Client = _client.Client.FirstOrDefault(client => client.Id == info.ClientId),
                // Product = _product.Product.FirstOrDefault(product => product.Id == info.ProductId),
                ClientId = info.ClientId,
                ProductId = info.ProductId,
                Time = info.Time
            };
            
            await _transaction.AddAsync(transaction);
            await _transaction.SaveChangesAsync();
            return new OkObjectResult($"Added transaction for product {info.ProductId}, client {info.ClientId} to database!");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]
        [Route("List")]
        public IActionResult ListTransaction()
        {
            var transactions = from t in _transaction.Transactions select t;
            return new OkObjectResult(transactions);
        }
    }
}