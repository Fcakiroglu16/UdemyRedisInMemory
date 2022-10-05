using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RedisExampleApp.API.Models;
using RedisExampleApp.API.Repositories;
using RedisExampleApp.API.Services;
using RedisExampleApp.Cache;
using StackExchange.Redis;

namespace RedisExampleApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _productService.GetAsync());
        }
        // www.api.com/products/10
        [HttpGet("{id}")]
        public async Task<IActionResult>  GetById(int id)
        {

            return Ok(await _productService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {

            return Created(string.Empty,await _productService.CreateAsync(product));
        }




    }
}
