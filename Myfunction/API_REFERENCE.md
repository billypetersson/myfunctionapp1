# API Reference - Azure Functions Product API

## Base URL
```
https://<function-app-name>.azurewebsites.net/api
```

## Authentication

All endpoints require function-level authentication using one of the following methods:

### Query Parameter
```
GET /api/products?code=<function-key>
```

### Request Header
```
x-functions-key: <function-key>
```

## Content Type

All requests and responses use JSON format:
```
Content-Type: application/json
```

---

## Endpoints

### 1. Create Product

Creates a new product in the system.

**Function Name:** `CreateProduct` (implemented in CreateProduct.cs)

#### Request
```http
POST /api/products
Content-Type: application/json
x-functions-key: <function-key>

{
    "name": "string",
    "description": "string",
    "price": number,
    "category": "string"
}
```

#### Request Parameters

Based on `CreateProductRequest.cs`:

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `name` | string | Yes | 1-100 characters, Required | Product name |
| `description` | string | No | Max 500 characters | Product description |
| `price` | decimal | Yes | > 0.01, Range validation | Product price |
| `category` | string | Yes | 1-50 characters, Required | Product category |

#### Validation Rules
- **Name**: Required field, minimum 1 character, maximum 100 characters
- **Description**: Optional field, maximum 500 characters
- **Price**: Required field, must be greater than 0 (custom error: "Price must be greater than 0")
- **Category**: Required field, minimum 1 character, maximum 50 characters

#### Response

**Success (201 Created)**
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Premium Coffee Mug",
    "description": "High-quality ceramic coffee mug with ergonomic handle",
    "price": 24.99,
    "category": "Kitchenware",
    "createdAt": "2025-06-16T10:30:00.000Z",
    "isActive": true
}
```

**Validation Error (400 Bad Request)**
```json
{
    "error": "ValidationFailed",
    "message": "One or more validation errors occurred",
    "validationErrors": {
        "Name": ["The Name field is required."],
        "Price": ["Price must be greater than 0"]
    }
}
```

**JSON Error (400 Bad Request)**
```json
{
    "error": "BadRequest",
    "message": "Invalid JSON format"
}
```

**Empty Body Error (400 Bad Request)**
```json
{
    "error": "BadRequest",
    "message": "Request body cannot be empty"
}
```

#### cURL Example
```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/products" \
  -H "Content-Type: application/json" \
  -H "x-functions-key: your-function-key" \
  -d '{
    "name": "Premium Coffee Mug",
    "description": "High-quality ceramic coffee mug",
    "price": 24.99,
    "category": "Kitchenware"
  }'
```

---

### 2. Get All Products

Retrieves all products from the system.

**Function Name:** `GetAllProducts` (implemented in GetAllProducts.cs)

#### Request
```http
GET /api/products
x-functions-key: <function-key>
```

#### Response

**Success (200 OK)**
```json
{
    "products": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "name": "Premium Coffee Mug",
            "description": "High-quality ceramic coffee mug with ergonomic handle",
            "price": 24.99,
            "category": "Kitchenware",
            "createdAt": "2025-06-16T10:30:00.000Z",
            "isActive": true
        },
        {
            "id": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
            "name": "Wireless Keyboard",
            "description": "Bluetooth wireless keyboard with backlight",
            "price": 89.99,
            "category": "Electronics",
            "createdAt": "2025-06-16T11:15:00.000Z",
            "isActive": true
        }
    ],
    "count": 2,
    "retrievedAt": "2025-06-16T12:00:00.000Z"
}
```

**Empty Result (200 OK)**
```json
{
    "products": [],
    "count": 0,
    "retrievedAt": "2025-06-16T12:00:00.000Z"
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `products` | array | Array of product objects |
| `count` | integer | Number of products returned (calculated via `.Count()`) |
| `retrievedAt` | string (ISO 8601) | UTC timestamp when data was retrieved |

#### Product Object Fields

Based on `ProductResponse.cs`:

| Field | Type | Description |
|-------|------|-------------|
| `id` | string (GUID) | Unique product identifier (from RowKey) |
| `name` | string | Product name |
| `description` | string | Product description (may be empty) |
| `price` | decimal | Product price |
| `category` | string | Product category |
| `createdAt` | string (ISO 8601) | Product creation timestamp (UTC) |
| `isActive` | boolean | Product active status (always true for created products) |

#### Data Ordering
Products are returned ordered by `CreatedAt` in descending order (newest first).

#### cURL Example
```bash
curl -X GET "https://your-function-app.azurewebsites.net/api/products" \
  -H "x-functions-key: your-function-key"
```

---

## Error Responses

### Standard Error Format

Based on `ApiErrorResponse.cs`:

```json
{
    "error": "ErrorType",
    "message": "Human-readable error message"
}
```

### Validation Error Format

For validation errors (400 Bad Request):

```json
{
    "error": "ValidationFailed",
    "message": "One or more validation errors occurred",
    "validationErrors": {
        "FieldName": ["Error message 1", "Error message 2"]
    }
}
```

### HTTP Status Codes

| Status Code | Description | When It Occurs |
|-------------|-------------|----------------|
| 200 OK | Success | Successful GET requests |
| 201 Created | Resource created | Successful POST requests |
| 400 Bad Request | Client error | Invalid JSON, validation failures, empty body |
| 401 Unauthorized | Authentication required | Missing or invalid function key |
| 500 Internal Server Error | Server error | System failures, database errors, unexpected exceptions |

### Common Error Scenarios

#### Authentication Errors
```json
{
    "error": "Unauthorized",
    "message": "Access denied. Invalid function key."
}
```

#### Validation Errors
```json
{
    "error": "ValidationFailed",
    "message": "One or more validation errors occurred",
    "validationErrors": {
        "Name": ["The Name field is required."],
        "Price": ["Price must be greater than 0"],
        "Category": ["The Category field is required."]
    }
}
```

#### Server Errors
```json
{
    "error": "InternalServerError",
    "message": "An unexpected error occurred"
}
```

---

## Code Examples

### JavaScript/Node.js

```javascript
const API_BASE_URL = 'https://your-function-app.azurewebsites.net/api';
const FUNCTION_KEY = 'your-function-key';

// Create product
const createProduct = async (productData) => {
    try {
        const response = await fetch(`${API_BASE_URL}/products`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'x-functions-key': FUNCTION_KEY
            },
            body: JSON.stringify(productData)
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(`API Error: ${error.message}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Failed to create product:', error);
        throw error;
    }
};

// Get all products
const getAllProducts = async () => {
    try {
        const response = await fetch(`${API_BASE_URL}/products`, {
            headers: {
                'x-functions-key': FUNCTION_KEY
            }
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Failed to get products:', error);
        throw error;
    }
};

// Usage examples
const newProduct = {
    name: 'Wireless Mouse',
    description: 'Ergonomic wireless mouse with long battery life',
    price: 29.99,
    category: 'Electronics'
};

createProduct(newProduct)
    .then(product => console.log('Created product:', product))
    .catch(error => console.error('Error:', error));

getAllProducts()
    .then(response => {
        console.log(`Found ${response.count} products`);
        console.log('Products:', response.products);
    })
    .catch(error => console.error('Error:', error));
```

### C#

```csharp
using System.Text.Json;

public class ProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _functionKey;
    private readonly string _baseUrl;
    
    public ProductApiClient(HttpClient httpClient, string baseUrl, string functionKey)
    {
        _httpClient = httpClient;
        _functionKey = functionKey;
        _baseUrl = baseUrl;
        _httpClient.DefaultRequestHeaders.Add("x-functions-key", functionKey);
    }
    
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/products", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductResponse>(responseJson) 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to create product", ex);
        }
    }
    
    public async Task<GetProductsResponse> GetAllProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GetProductsResponse>(responseJson)
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to get products", ex);
        }
    }
}

// Data classes matching your API
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class GetProductsResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int Count { get; set; }
    public DateTime RetrievedAt { get; set; }
}
```

### Python

```python
import requests
import json
from typing import Dict, List, Optional
from datetime import datetime

class ProductApiClient:
    def __init__(self, base_url: str, function_key: str):
        self.base_url = base_url.rstrip('/')
        self.headers = {
            'Content-Type': 'application/json',
            'x-functions-key': function_key
        }
    
    def create_product(self, product_data: Dict) -> Dict:
        """Create a new product"""
        try:
            response = requests.post(
                f"{self.base_url}/api/products",
                headers=self.headers,
                json=product_data,
                timeout=30
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            raise Exception(f"Failed to create product: {e}")
    
    def get_all_products(self) -> Dict:
        """Get all products"""
        try:
            response = requests.get(
                f"{self.base_url}/api/products",
                headers=self.headers,
                timeout=30
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            raise Exception(f"Failed to get products: {e}")

# Usage example
if __name__ == "__main__":
    client = ProductApiClient(
        'https://your-function-app.azurewebsites.net', 
        'your-function-key'
    )
    
    # Create a product
    new_product = {
        'name': 'Smart Watch',
        'description': 'Fitness tracking smartwatch with heart rate monitor',
        'price': 199.99,
        'category': 'Electronics'
    }
    
    try:
        created_product = client.create_product(new_product)
        print(f"Created product: {created_product['name']} with ID: {created_product['id']}")
        
        # Get all products
        products_response = client.get_all_products()
        print(f"Total products: {products_response['count']}")
        
        for product in products_response['products']:
            print(f"- {product['name']}: ${product['price']}")
            
    except Exception as e:
        print(f"Error: {e}")
```

---

## Implementation Details

### Function Configuration

Based on your `host.json`:
- **Function timeout**: 5 minutes
- **Route prefix**: "api"
- **Application Insights**: Enabled with sampling
- **Logging**: Information level with ASP.NET Core warnings

### Local Development

Based on your `launchSettings.json`:
- **Default port**: 7133
- **Command**: `func start --port 7133`

### Package Dependencies

Your project uses these key packages:
- `Azure.Data.Tables` (12.11.0) - Table Storage operations
- `Microsoft.Azure.Functions.Worker` (2.0.0) - Function runtime
- `Microsoft.Azure.Functions.Worker.ApplicationInsights` (2.0.0) - Telemetry

### Storage Schema

Products are stored in Azure Table Storage with:
- **Table Name**: "products" (auto-created)
- **Partition Key**: "Products" (constant)
- **Row Key**: GUID string (unique product ID)

---

## Rate Limits and Performance

### Azure Functions Limits
- **Consumption Plan**: Dynamic scaling with built-in throttling
- **Timeout**: 5 minutes (configured in host.json)
- **Concurrent Executions**: Based on your plan limits

### Typical Response Times
- **Cold Start**: 1-3 seconds (first request after idle)
- **Warm Requests**: 50-200ms
- **Create Product**: 100-500ms (includes database write)
- **Get All Products**: 50-300ms (depends on product count)

### Data Limits
- **Request Body**: 100MB maximum (Azure Functions limit)
- **Product Name**: 100 characters maximum
- **Product Description**: 500 characters maximum
- **Product Category**: 50 characters maximum
- **Price**: