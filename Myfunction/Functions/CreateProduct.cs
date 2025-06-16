using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Myfunction.Models.Requests;
using Myfunction.Models.Responses;
using Myfunction.Services;
using Myfunction.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Myfunction.Functions
{
    public class ProductFunction
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductFunction> _logger;

        public ProductFunction(IProductService productService, ILogger<ProductFunction> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CreateProduct")]
        public async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Processing create product request");

                // Read and deserialize JSON request
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult(new ApiErrorResponse
                    {
                        Error = "BadRequest",
                        Message = "Request body cannot be empty"
                    });
                }

                CreateProductRequest? request;
                try
                {
                    request = JsonSerializer.Deserialize<CreateProductRequest>(requestBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid JSON in request body");
                    return new BadRequestObjectResult(new ApiErrorResponse
                    {
                        Error = "BadRequest",
                        Message = "Invalid JSON format"
                    });
                }

                if (request == null)
                {
                    return new BadRequestObjectResult(new ApiErrorResponse
                    {
                        Error = "BadRequest",
                        Message = "Request cannot be null"
                    });
                }

                // Validate request
                var validationResults = ProductValidator.ValidateRequest(request);
                if (validationResults.Any())
                {
                    return new BadRequestObjectResult(new ApiErrorResponse
                    {
                        Error = "ValidationFailed",
                        Message = "One or more validation errors occurred",
                        ValidationErrors = validationResults
                    });
                }

                // Create product
                var product = await _productService.CreateProductAsync(request);

                return new CreatedResult($"/api/products/{product.Id}", product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateProduct");
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
}
