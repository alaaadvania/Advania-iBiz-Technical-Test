using ProductApi.Models;

namespace ProductApi.Services;

public interface IProductService
{
    Task AddProductAsync(Product product);

    Task<IReadOnlyList<Product>> GetProductsAsync();
}
