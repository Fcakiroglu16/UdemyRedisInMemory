using RedisExampleApp.API.Models;

namespace RedisExampleApp.API.Services
{
    public interface IProductService
    {

        Task<List<Product>> GetAsync();

        Task<Product> GetByIdAsync(int id);

        Task<Product> CreateAsync(Product product);
    }
}
