# Low Level Design - Azure Functions Product API

## 1. System Architecture

### 1.1 Component Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   HTTP Client   │────│ Azure Functions │────│ Azure Table     │
│   (REST API)    │    │   Runtime       │    │   Storage       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                       ┌─────────────────┐
                       │ Application     │
                       │   Insights      │
                       └─────────────────┘
```

### 1.2 Detailed Component Architecture

```
┌─────────────────── Azure Functions Host ───────────────────┐
│                                                            │
│  ┌── HTTP Triggers ──┐  ┌── Business Logic ──┐            │
│  │                   │  │                    │            │
│  │ ProductFunction   │──│ IProductService    │            │
│  │ - CreateProduct   │  │                    │            │
│  │ - GetAllProducts  │  │ ProductService     │            │
│  │                   │  │ - CreateAsync      │            │
│  └───────────────────┘  │ - GetAllAsync      │            │
│                         └────────────────────┘            │
│                                   │                       │
│  ┌── Data Access ──┐              │                       │
│  │                 │              │                       │
│  │ TableClient     │◄─────────────┘                       │
│  │ (Azure SDK)     │                                      │
│  └─────────────────┘                                      │
└────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────────────┐
                    │ Azure Table     │
                    │   Storage       │
                    │                 │
                    │ Table: products │
                    └─────────────────┘
```

## 2. Class Design

### 2.1 Class Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       ProductFunction                       │
├─────────────────────────────────────────────────────────────┤
│ - _productService: IProductService                          │
│ - _logger: ILogger<ProductFunction>                         │
├─────────────────────────────────────────────────────────────┤
│ + CreateProduct(HttpRequest): Task<IActionResult>           │
│ + GetAllProducts(HttpRequest): Task<IActionResult>          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    <<interface>>                            │
│                    IProductService                          │
├─────────────────────────────────────────────────────────────┤
│ + CreateProductAsync(CreateProductRequest):                 │
│   Task<ProductResponse>                                     │
│ + GetAllProductsAsync(): Task<IEnumerable<ProductResponse>> │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     ProductService                          │
├─────────────────────────────────────────────────────────────┤
│ - _tableClient: TableClient                                 │
│ - _logger: ILogger<ProductService>                          │
├─────────────────────────────────────────────────────────────┤
│ + CreateProductAsync(CreateProductRequest):                 │
│   Task<ProductResponse>                                     │
│ + GetAllProductsAsync(): Task<IEnumerable<ProductResponse>> │
│ - MapToResponse(ProductEntity): ProductResponse             │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Data Models

```
┌─────────────────────────────────────────────────────────────┐
│                 CreateProductRequest                        │
├─────────────────────────────────────────────────────────────┤
│ + Name: string [Required, StringLength(100)]                │
│ + Description: string [StringLength(500)]                   │
│ + Price: decimal [Required, Range(0.01, MaxValue)]          │
│ + Category: string [Required, StringLength(50)]             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   ProductResponse                           │
├─────────────────────────────────────────────────────────────┤
│ + Id: string                                                │
│ + Name: string                                              │
│ + Description: string                                       │
│ + Price: decimal                                            │
│ + Category: string                                          │
│ + CreatedAt: DateTime                                       │
│ + IsActive: bool                                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│              ProductEntity : ITableEntity                   │
├─────────────────────────────────────────────────────────────┤
│ + PartitionKey: string = "Products"                         │
│ + RowKey: string                                            │
│ + Timestamp: DateTimeOffset?                                │
│ + ETag: Azure.ETag                                          │
│ + Name: string                                              │
│ + Description: string                                       │
│ + Price: decimal                                            │
│ + Category: string                                          │
│ + CreatedAt: DateTime                                       │
│ + IsActive: bool                                            │
└─────────────────────────────────────────────────────────────┘
```

## 3. Sequence Diagrams

### 3.1 Create Product Flow

```
Client          ProductFunction    ProductService    TableClient    Azure Table Storage
  │                    │                 │               │               │
  │ POST /api/products │                 │               │               │
  ├───────────────────►│                 │               │               │
  │                    │ ValidateRequest │               │               │
  │                    ├────────────────►│               │               │
  │                    │                 │ CreateAsync   │               │
  │                    │                 ├──────────────►│               │
  │                    │                 │               │ AddEntityAsync│
  │                    │                 │               ├──────────────►│
  │                    │                 │               │               │
  │                    │                 │               │ Success       │
  │                    │                 │               │◄──────────────│
  │                    │                 │ ProductResponse│              │
  │                    │                 │◄──────────────│               │
  │                    │ 201 Created     │               │               │
  │◄───────────────────│                 │               │               │
```

### 3.2 Get All Products Flow

```
Client          ProductFunction    ProductService    TableClient    Azure Table Storage
  │                    │                 │               │               │
  │ GET /api/products  │                 │               │               │
  ├───────────────────►│                 │               │               │
  │                    │ GetAllAsync     │               │               │
  │                    ├────────────────►│               │               │
  │                    │                 │ QueryAsync    │               │
  │                    │                 ├──────────────►│               │
  │                    │                 │               │ Query Entities│
  │                    │                 │               ├──────────────►│
  │                    │                 │               │               │
  │                    │                 │               │ Entity List   │
  │                    │                 │               │◄──────────────│
  │                    │                 │ Response List │               │
  │                    │                 │◄──────────────│               │
  │                    │ 200 OK          │               │               │
  │◄───────────────────│                 │               │               │
```

## 4. Data Flow Design

### 4.1 Request/Response Processing

```
┌─── HTTP Request ───┐
│                    │
│ Headers:           │
│ - Content-Type     │
│ - Authorization    │
│                    │
│ Body:              │
│ - JSON Payload     │
└────────────────────┘
         │
         ▼
┌─── Function Trigger ───┐
│                        │
│ 1. Route Resolution    │
│ 2. Auth Validation     │
│ 3. Request Binding     │
└────────────────────────┘
         │
         ▼
┌─── Input Validation ───┐
│                        │
│ 1. JSON Deserialization│
│ 2. Model Validation    │
│ 3. Business Rules      │
└────────────────────────┘
         │
         ▼
┌─── Business Logic ─────┐
│                        │
│ 1. Service Method Call │
│ 2. Data Transformation │
│ 3. Storage Operations  │
└────────────────────────┘
         │
         ▼
┌─── Response Formation ─┐
│                        │
│ 1. Model Mapping       │
│ 2. Status Code Setting │
│ 3. JSON Serialization  │
└────────────────────────┘
```

### 4.2 Error Handling Flow

```
Exception
    │
    ├─── ValidationException
    │    └─── 400 Bad Request + Validation Errors
    │
    ├─── JsonException
    │    └─── 400 Bad Request + "Invalid JSON"
    │
    ├─── ArgumentNullException
    │    └─── 400 Bad Request + "Required field missing"
    │
    ├─── TableStorageException
    │    └─── 500 Internal Server Error + Generic Message
    │
    └─── Generic Exception
         └─── 500 Internal Server Error + Generic Message
```

## 5. Database Design

### 5.1 Azure Table Storage Schema

```
Table Name: products
┌─────────────────┬─────────────────┬──────────────┬─────────────────┐
│   PartitionKey  │     RowKey      │   Property   │      Type       │
├─────────────────┼─────────────────┼──────────────┼─────────────────┤
│   "Products"    │  {GUID-String}  │     Name     │     String      │
│   (Constant)    │   (Unique ID)   │ Description  │     String      │
│                 │                 │    Price     │ Decimal/Double  │
│                 │                 │   Category   │     String      │
│                 │                 │  CreatedAt   │    DateTime     │
│                 │                 │   IsActive   │     Boolean     │
│                 │                 │  Timestamp   │ DateTimeOffset  │
│                 │                 │     ETag     │     String      │
└─────────────────┴─────────────────┴──────────────┴─────────────────┘
```

### 5.2 Query Patterns

**Create Operation:**
```
Operation: AddEntity
PartitionKey: "Products"
RowKey: {Generated GUID}
Data: {Product Properties}
```

**Read All Operation:**
```
Operation: Query
Filter: PartitionKey eq 'Products'
Result: All products in partition
Ordering: Client-side by CreatedAt DESC
```

### 5.3 Indexing Strategy

**Primary Index (Automatic):**
- PartitionKey + RowKey (Clustered Index)
- Used for: Direct lookups, Partition scans

**Query Optimization:**
- Single partition design = fast queries within partition
- Trade-off: Limited scalability for massive datasets
- Future consideration: Partition by Category or Date

## 6. API Design Details

### 6.1 HTTP Method Mapping

```
┌─────────────┬─────────────────┬─────────────────┬──────────────────┐
│   Method    │      Route      │   Function      │   Description    │
├─────────────┼─────────────────┼─────────────────┼──────────────────┤
│    POST     │ /api/products   │ CreateProduct   │ Create new       │
│    GET      │ /api/products   │ GetAllProducts  │ Get all products │
└─────────────┴─────────────────┴─────────────────┴──────────────────┘
```

### 6.2 Content Type Handling

**Request:**
- Content-Type: application/json
- Accept: application/json

**Response:**
- Content-Type: application/json
- Character encoding: UTF-8

### 6.3 Status Code Matrix

```
┌──────────────┬─────────────┬─────────────┬─────────────────────┐
│  Operation   │   Success   │    Error    │     Description     │
├──────────────┼─────────────┼─────────────┼─────────────────────┤
│ CreateProduct│ 201 Created │ 400/500     │ Location header set │
│ GetProducts  │ 200 OK      │ 500         │ Array response      │
│ Validation   │ N/A         │ 400         │ Detailed errors     │
│ Server Error │ N/A         │ 500         │ Generic message     │
└──────────────┴─────────────┴─────────────┴─────────────────────┘
```

## 7. Security Design

### 7.1 Authentication Flow

```
Client Request
    │
    ├─── Function Key Validation
    │    └─── Azure Functions Runtime
    │         ├─── Valid Key → Continue
    │         └─── Invalid Key → 401 Unauthorized
    │
    └─── Request Processing
         └─── Business Logic Execution
```

### 7.2 Input Sanitization

```
Raw Input
    │
    ├─── JSON Schema Validation
    │    ├─── Valid → Continue
    │    └─── Invalid → 400 Bad Request
    │
    ├─── Data Annotation Validation
    │    ├─── [Required] checks
    │    ├─── [StringLength] checks
    │    ├─── [Range] checks
    │    └─── Custom validation
    │
    └─── Business Rule Validation
         └─── Custom logic validation
```

## 8. Performance Considerations

### 8.1 Scalability Design

**Horizontal Scaling:**
- Azure Functions: Auto-scale based on demand
- Table Storage: Partition-based scaling
- Concurrent execution: Thread-safe operations

**Performance Bottlenecks:**
- Single partition design limits throughput
- GetAll operation fetches all records
- No caching layer implemented

### 8.2 Optimization Strategies

**Current Implementation:**
```
Client → Function → Table Storage
    ↑
Single round-trip per operation
```

**Potential Optimizations:**
```
Client → Function → Cache → Table Storage
    ↑              ↑
Response Cache   Write-through
```

## 9. Error Handling Design

### 9.1 Exception Hierarchy

```
System.Exception
    │
    ├─── ArgumentException
    │    ├─── ArgumentNullException
    │    └─── ArgumentOutOfRangeException
    │
    ├─── System.Text.Json.JsonException
    │
    ├─── Azure.RequestFailedException
    │    └─── TableStorageException
    │
    └─── ValidationException (Custom)
```

### 9.2 Error Response Schema

```json
{
    "error": "string",           // Error category
    "message": "string",         // Human-readable message
    "validationErrors": {        // Optional field-specific errors
        "fieldName": ["error1", "error2"]
    }
}
```

## 10. Logging and Monitoring Design

### 10.1 Logging Levels

```
┌─────────────┬─────────────────────────────────────────────────────┐
│    Level    │                   Usage                             │
├─────────────┼─────────────────────────────────────────────────────┤
│ Information │ Normal flow, successful operations                  │
│ Warning     │ Validation failures, business rule violations       │
│ Error       │ Exceptions, system failures                         │
│ Debug       │ Detailed execution flow (dev/test only)             │
└─────────────┴─────────────────────────────────────────────────────┘
```

### 10.2 Telemetry Data

**Custom Metrics:**
- Request count per endpoint
- Response time percentiles
- Error rate by type
- Product creation rate

**Structured Logging:**
```json
{
    "timestamp": "2025-06-14T10:30:00Z",
    "level": "Information",
    "message": "Product created successfully",
    "properties": {
        "productId": "guid",
        "operation": "CreateProduct",
        "executionTime": "150ms"
    }
}
```

## 11. Deployment Architecture

### 11.1 Resource Dependencies

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Function App    │────│ Storage Account │────│ App Insights    │
│ - .NET 9        │    │ - Table Storage │    │ - Telemetry     │
│ - Consumption   │    │ - Blob Storage  │    │ - Metrics       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 11.2 Configuration Management

**Environment Variables:**
```
AzureWebJobsStorage={connection-string}
APPLICATIONINSIGHTS_CONNECTION_STRING={app-insights-string}
FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
```

**Application Settings:**
- Function timeout: 5 minutes
- Runtime version: ~4
- .NET version: 9.0

Detta low-level design ger dig en detaljerad teknisk grund för implementering, optimering och diskussioner om systemarkitektur!
