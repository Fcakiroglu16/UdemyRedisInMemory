using RedisExampleApp.API.Models;

namespace RedisExampleApp.API.Repositories
{
    public interface IProductRepository
    {

        Task<List<Product>> GetAsync();

        Task<Product> GetByIdAsync(int id);

        Task<Product> CreateAsync(Product product);
    }
}
