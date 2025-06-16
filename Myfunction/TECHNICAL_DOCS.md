# Technical Documentation - Azure Functions Product API

## Architecture Overview

### System Architecture
```
Client Applications
       ↓
   Azure Functions
       ↓
Azure Table Storage
       ↓
Application Insights
```

### Technology Stack
- **Runtime**: .NET 9
- **Platform**: Azure Functions v4
- **Database**: Azure Table Storage
- **Monitoring**: Application Insights
- **Authentication**: Function-level keys

### Design Principles
- **Serverless-first**: Leverages Azure Functions for automatic scaling
- **NoSQL storage**: Uses Azure Table Storage for simple, fast operations
- **Stateless**: Each function execution is independent
- **Event-driven**: HTTP-triggered functions with JSON payloads

## Code Structure Analysis

### Actual Project Structure
```
Myfunction/
├── Functions/
│   ├── CreateProduct.cs       
│   └── GetAllProducts.cs       
├── Models/
│   ├── Requests/                
│   │   └── CreateProductRequest.cs
│   ├── Responses/               
│   │   ├── ProductResponse.cs
│   │   └── ApiErrorResponse.cs
│   └── TableEntities/          
│       └── ProductEntity.cs    
├── Services/
│   ├── IProductService.cs
│   └── ProductService.cs      
├── Static/
│   └── ProductValidator.cs
└── Configuration files...
```

## Data Architecture

### Storage Strategy
Azure Table Storage was chosen for:
- **Simplicity**: No complex relationships or joins required
- **Performance**: Fast read/write operations for product data
- **Cost**: Pay-per-transaction model aligns with serverless approach
- **Scalability**: Automatic partitioning and load distribution

### Data Model Design

#### ProductEntity Schema (from ProductEntity.cs)
```csharp
public class ProductEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Products";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

#### Partitioning Strategy
- **Single Partition**: All products use "Products" as PartitionKey
- **Rationale**: Simple queries, acceptable for moderate data volumes
- **Scalability Considerations**: May need repartitioning for large datasets

### Validation Architecture

#### Input Validation Pipeline
1. **JSON Deserialization**: Automatic with error handling
2. **Data Annotations**: Declarative validation rules
3. **Custom Validation**: Business logic validation via ProductValidator
4. **Error Aggregation**: Centralized error response formatting

#### Validation Rules (from CreateProductRequest.cs)
```csharp
public class CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;
}
```

## Function Implementation Details

### CreateProduct Function
Located in `Functions/CreateProduct.cs`, this function:
- Handles POST requests to `/api/products`
- Performs comprehensive JSON validation
- Uses ProductValidator for business rule validation
- Returns detailed validation errors when input is invalid
- Creates products via IProductService dependency injection

### GetAllProducts Function
Located in `Functions/GetAllProducts.cs`, this function:
- Handles GET requests to `/api/products`
- Retrieves all products via IProductService
- Returns products with metadata (count, retrievedAt timestamp)
- Includes proper error handling for service failures

### Service Layer Implementation

#### IProductService Interface
```csharp
public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
}
```

#### ProductService Implementation
Key features:
- Uses Azure.Data.Tables SDK (version 12.11.0)
- Implements proper async/await patterns
- Includes comprehensive logging
- Maps between domain models and table entities
- Orders results by CreatedAt (descending)

## Security Architecture

### Authentication & Authorization
- **Function-level Keys**: Each function protected by unique keys
- **Key Management**: Managed through Azure portal or ARM templates
- **HTTPS Enforcement**: All communication encrypted in transit
- **No User Authentication**: Simplified security model for internal APIs

### Input Security
- **JSON Schema Validation**: Prevents malformed data
- **Data Type Validation**: Ensures proper data types
- **Length Validation**: Prevents oversized inputs
- **Business Rule Validation**: Custom validation logic

## Error Handling Strategy

### Error Response Format (from ApiErrorResponse.cs)
```csharp
public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
```

### Error Classification
1. **Validation Errors** (400): Invalid input data with detailed field errors
2. **JSON Errors** (400): Malformed JSON or deserialization failures
3. **System Errors** (500): Infrastructure or code failures
4. **Missing Data** (400): Required fields not provided

### Logging Strategy
- **Structured Logging**: Uses ILogger<T> dependency injection
- **Log Levels**: Information for success, Warning for validation, Error for exceptions
- **Context**: Includes relevant data like product IDs and counts
- **Application Insights**: Automatic telemetry collection

## Performance Considerations

### Current Implementation Characteristics
- **Horizontal Scaling**: Automatic via Azure Functions
- **Cold Start**: ~1-3 seconds for .NET functions
- **Memory Usage**: Optimized with minimal dependencies
- **Async Operations**: Non-blocking I/O throughout

### Package Dependencies (from Myfunction.csproj)
```xml
<PackageReference Include="Azure.Data.Tables" Version="12.11.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="2.0.0" />
<!-- Additional packages for HTTP and SDK support -->
```

### Configuration (from host.json)
```json
{
  "version": "2.0",
  "functionTimeout": "00:05:00",
  "extensions": {
    "http": {
      "routePrefix": "api"
    }
  },
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  }
}
```

## Deployment Architecture

### Local Development Setup
- **Port**: 7133 (configured in launchSettings.json)
- **Storage Emulator**: Supported via serviceDependencies.local.json
- **Application Insights**: SDK mode for local development

### Environment Configuration
- **Development**: Uses Azure Storage Emulator
- **Production**: Uses actual Azure Storage Account
- **Service Dependencies**: Configured for both local and cloud environments

## Code Quality Issues and Recommendations

### Code Quality Strengths
- **Dependency Injection**: Proper use of IServiceCollection
- **Async/Await**: Consistent async patterns
- **Error Handling**: Comprehensive try-catch blocks
- **Logging**: Good use of structured logging
- **Validation**: Proper use of Data Annotations

## Future Architecture Considerations

### Scalability Enhancements
1. **Partitioning Strategy**: Implement category-based partitioning
2. **Caching Layer**: Redis for frequently accessed data
3. **API Gateway**: Azure API Management for enterprise features
4. **Database Migration**: Consider Cosmos DB for global distribution

### Feature Expansions
1. **CRUD Operations**: Add Update and Delete endpoints
2. **Search Functionality**: Product search and filtering
3. **Authentication**: JWT-based user authentication
4. **File Upload**: Product image handling

### Monitoring Evolution
1. **Custom Metrics**: Business-specific KPIs
2. **Health Checks**: Endpoint health monitoring
3. **Alerting**: Proactive issue detection
4. **Performance Baselines**: Establish SLA metrics

## Troubleshooting Guide

### Common Development Issues

#### Compilation Errors
- **Duplicate class names**: Rename classes to be unique
- **Namespace issues**: Fix typos in namespace declarations
- **Missing references**: Ensure all required packages are installed

#### Runtime Issues
- **Storage connection**: Verify AzureWebJobsStorage configuration
- **JSON deserialization**: Check request body format
- **Validation failures**: Review Data Annotation attributes

#### Deployment Issues
- **Function app configuration**: Verify .NET 9 runtime
- **Connection strings**: Ensure production settings are correct
- **Access permissions**: Check managed identity configuration

### Debugging Techniques
1. **Local Development**: Use Azure Functions Core Tools
2. **Remote Debugging**: Application Insights live metrics
3. **Log Analysis**: Structured logging in Application Insights
4. **Performance Profiling**: Function execution metrics

This technical documentation reflects the actual codebase structure and identifies areas for improvement while maintaining accuracy about the current implementation.
