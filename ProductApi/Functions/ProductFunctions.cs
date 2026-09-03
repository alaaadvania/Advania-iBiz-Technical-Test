using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductApi.Models;
using ProductApi.Services;
using System.Net;
using System.Text.Json;

namespace ProductApi.Functions;

public class ProductFunctions
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductFunctions> _logger;

    public ProductFunctions(IProductService productService, ILogger<ProductFunctions> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new product and stores it in Azure Table Storage.
    /// </summary>
    /// <param name="request">The HTTP request containing the product data.</param>
    /// <returns>
    /// A 201 Created response when the product is successfully created,
    /// or a 400 Bad Request response when the input is invalid.
    /// </returns>
    [Function("CreateProduct")]
    public async Task<HttpResponseData> CreateProduct(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "products")]
        HttpRequestData request)
    {
        try
        {
            var product = await JsonSerializer.DeserializeAsync<Product>(
                request.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (product == null)
            {
                var badRequest = request.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Request body is required.");
                
                _logger.LogWarning("Request body is required.");

                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                var badRequest = request.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Product name is required.");
                
                _logger.LogWarning("Product name is required.");

                return badRequest;
            }

            if (product.Price < 0)
            {
                var badRequest = request.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Product price cannot be negative.");
               
                _logger.LogWarning("Product price cannot be negative.");

                return badRequest;
            }

            await _productService.AddProductAsync(product);

            var response = request.CreateResponse(
                HttpStatusCode.Created);

            await response.WriteAsJsonAsync(product);

            return response;
        }
        catch (JsonException)
        {
            var response = request.CreateResponse(
                HttpStatusCode.BadRequest);

            await response.WriteStringAsync(
                "Invalid JSON.");

            _logger.LogWarning("Invalid JSON in request body.");
            return response;
        }
        catch (Exception)
        {
            var response = request.CreateResponse(
                HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                "An unexpected error occurred.");

            _logger.LogError("An unexpected error occurred while creating a product.");
            return response;
        }
    }

    /// <summary>
    /// Retrieves all products stored in Azure Table Storage.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>
    /// A 200 OK response containing all products,
    /// or a 500 Internal Server Error if an unexpected error occurs.
    /// </returns>
    [Function("GetProducts")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "products")]
        HttpRequestData request)
    {
        try
        {
            var products = await _productService.GetProductsAsync();

            var response = request.CreateResponse(
                HttpStatusCode.OK);

            await response.WriteAsJsonAsync(products);

            return response;
        }
        catch (Exception)
        {
            var response = request.CreateResponse(
                HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                "An unexpected error occurred.");

            _logger.LogError("An unexpected error occurred while retrieving products.");

            return response;
        }
    }
}
