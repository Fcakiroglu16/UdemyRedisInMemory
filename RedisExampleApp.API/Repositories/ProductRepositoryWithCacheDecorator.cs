
using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {

        private const string productKey = "productCaches";
        private readonly IProductRepository _productRepository;
        private readonly RedisService _rediService;
        private readonly IDatabase _cacheRepository;
        public ProductRepositoryWithCacheDecorator(IProductRepository productRepository, RedisService rediService)
        {
            _productRepository = productRepository;
            _rediService = rediService;
            _cacheRepository = _rediService.GetDb(2);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _productRepository.CreateAsync(product);

            if (await _cacheRepository.KeyExistsAsync(productKey))
            {
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));
            }

            return  newProduct;

        }

        public async Task<List<Product>> GetAsync()
        {

            if (!await _cacheRepository.KeyExistsAsync(productKey))
                return await LoadToCacheFromDbAsync();

            var products =new List<Product>();

            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
            foreach (var item in   cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);

                products.Add(product);

            }

            return products;



        }

        public async Task<Product> GetByIdAsync(int id)
        {
           
            if(_cacheRepository.KeyExists(productKey))
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }


            var products = await LoadToCacheFromDbAsync();
            return products.FirstOrDefault(x => x.Id == id);



        }

        private async Task<List<Product>> LoadToCacheFromDbAsync()
        {

            var products = await _productRepository.GetAsync();


            products.ForEach(p =>
            {
                _cacheRepository.HashSetAsync(productKey, p.Id, JsonSerializer.Serialize(p));

            });

            return products;


        }




    }
}
