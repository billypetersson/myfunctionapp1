# Azure Functions Product API

A serverless REST API for product management built with Azure Functions, .NET 9, and Azure Table Storage.

## Features

- Create and retrieve products
- Serverless architecture with automatic scaling
- JSON validation and error handling
- Application Insights monitoring
- Function-level security

## Quick Start

### Prerequisites
- Azure Storage Account
- Azure Functions App (.NET 9, v4)
- Application Insights instance

### Local Development
1. Clone the repository
2. Install Azure Functions Core Tools
3. Configure local settings:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "your-storage-connection-string",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "APPLICATIONINSIGHTS_CONNECTION_STRING": "your-appinsights-connection"
     }
   }
   ```
4. Run locally:
   ```bash
   func start --port 7133
   ```

### Deploy to Azure
```bash
dotnet build
func azure functionapp publish <function-app-name>
```

## API Usage

### Create Product
```http
POST /api/products
Content-Type: application/json

{
    "name": "Product Name",
    "description": "Product Description",
    "price": 99.99,
    "category": "Category"
}
```

### Get All Products
```http
GET /api/products
```

## Authentication

All endpoints require function-level keys. Include the key in requests:
- Query parameter: `?code=<function-key>`
- Header: `x-functions-key: <function-key>`

## Project Structure

```
Myfunction/
├── Functions/
│   ├── CreateProduct.cs    # CreateProduct function
│   └── GetAllProducts.cs   # GetAllProducts function
├── Models/
│   ├── Requests/
│   │   └── CreateProductRequest.cs
│   ├── Responses/            # Note: Contains typo in original
│   │   ├── ProductResponse.cs
│   │   └── ApiErrorResponse.cs
│   └── TableEntities/
│       └── ProducEntity.cs # ProductEntity for Table Storage
├── Services/
│   ├── IProductService.cs  # Service interface
│   └── ProductService.cs   # Service implementation
├── Static/
│   └── ProductValidator.cs # Input validation
├── Properties/
│   ├── launchSettings.json
│   ├── serviceDependencies.json
│   └── serviceDependencies.local.json
├── host.json              # Function app configuration
├── Myfunction.csproj      # Project file
└── .gitignore
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `AzureWebJobsStorage` | Azure Storage connection string |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights configuration |

## Function Configuration

The app uses the following configuration in `host.json`:
- Function timeout: 5 minutes
- Route prefix: "api"
- Application Insights sampling enabled

## Known Issues to Fix

1. **Duplicate class names**: Both function files contain `ProductFunction` class
2. **Namespace typo**: `Respons` should be `Responses`
3. **Missing imports**: Some using statements may be missing

## Documentation

- 📋 [Complete Technical Documentation](TECHNICAL_DOCS.md) - Detailed architecture, data models, and advanced configuration
- 🔗 [API Reference](API_REFERENCE.md) - Complete endpoint documentation with examples

## Monitoring

Monitor your application through:
- Azure Functions portal
- Application Insights dashboard
- Function logs and metrics

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
