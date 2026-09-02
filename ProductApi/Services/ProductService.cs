using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private const string TableName = "Products";

    private readonly TableClient _tableClient;

    public ProductService(IConfiguration configuration)
    {
        var connectionString =
            configuration["AzureWebJobsStorage"]
            ?? throw new InvalidOperationException(
                "AzureWebJobsStorage is not configured.");

        _tableClient = new TableClient(
            connectionString,
            TableName);

        _tableClient.CreateIfNotExists();
    }

    public async Task AddProductAsync(Product product)
    {
        var entity = new ProductEntity
        {
            PartitionKey = "Products",
            RowKey = Guid.NewGuid().ToString(),
            Name = product.Name,
            Price = product.Price
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        var products = new List<Product>();

        await foreach (var entity in _tableClient.QueryAsync<ProductEntity>())
        {
            products.Add(new Product
            {
                Name = entity.Name,
                Price = entity.Price
            });
        }

        return products;
    }
}
