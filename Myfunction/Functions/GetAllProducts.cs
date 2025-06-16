using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Myfunction.Models.Responses;
using Myfunction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GetProductFunction
{
    private readonly IProductService _productService;
    private readonly ILogger<GetProductFunction> _logger;

    public GetProductFunction(IProductService productService, ILogger<GetProductFunction> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("GetAllProducts")]
    public async Task<IActionResult> GetAllProducts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Processing get all products request");

            var products = await _productService.GetAllProductsAsync();

            var response = new
            {
                Products = products,
                Count = products.Count(),
                RetrievedAt = DateTime.UtcNow
            };

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllProducts");
            return new ObjectResult(new ApiErrorResponse
            {
                Error = "InternalServerError",
                Message = "An unexpected error occurred"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
