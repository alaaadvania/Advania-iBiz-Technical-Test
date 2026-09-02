using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Functions;

public class ProductFunctions
{
    private readonly IProductService _productService;

    public ProductFunctions(IProductService productService)
    {
        _productService = productService;
    }

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

                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                var badRequest = request.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Product name is required.");

                return badRequest;
            }

            if (product.Price < 0)
            {
                var badRequest = request.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Product price cannot be negative.");

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

            return response;
        }
        catch (Exception)
        {
            var response = request.CreateResponse(
                HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                "An unexpected error occurred.");

            return response;
        }
    }

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

            return response;
        }
    }
}
