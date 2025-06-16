using Myfunction.Models.Requests;
using Myfunction.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myfunction.Services
{
    // Service for handling product operations
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
    }
}
