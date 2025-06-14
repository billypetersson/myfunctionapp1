using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Myfunction.Models.Request;
using Myfunction.Models.Respons;
using Myfunction.Models.TableEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myfunction.Services
{
    public class ProductService : IProductService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<ProductService> _logger;

        public ProductService(TableClient tableClient, ILogger<ProductService> logger)
        {
            _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                var productId = Guid.NewGuid().ToString();
                var entity = new ProductEntity
                {
                    RowKey = productId,
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Category = request.Category,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _tableClient.AddEntityAsync(entity);

                _logger.LogInformation("Product created successfully with ID: {ProductId}", productId);

                return MapToResponse(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            try
            {
                var products = new List<ProductResponse>();

                await foreach (var entity in _tableClient.QueryAsync<ProductEntity>(
                    filter: $"PartitionKey eq 'Products'"))
                {
                    products.Add(MapToResponse(entity));
                }

                _logger.LogInformation("Retrieved {Count} products", products.Count);

                return products.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }

        private static ProductResponse MapToResponse(ProductEntity entity)
        {
            return new ProductResponse
            {
                Id = entity.RowKey,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = entity.Category,
                CreatedAt = entity.CreatedAt,
                IsActive = entity.IsActive
            };
        }
    }
}
