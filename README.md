# Advania-iBiz-Technical-Test

Advania iBiz technical evaluation — C# Azure Function with HTTP APIs and Azure Table Storage.

## Product API

A simple Azure Functions API created as part of the Advania iBiz technical evaluation.

The API provides endpoints for creating and retrieving products using Azure Table Storage.

## Technologies

* C#
* .NET 8
* Azure Functions (Isolated Worker)
* Azure Table Storage
* Azurite for local development

## API Endpoints

### Create Product

**POST** `/api/products`

Creates and stores a new product in Azure Table Storage.

**Request:**

```json
{
  "name": "Keyboard",
  "price": 79.99
}
```

**Response:** `201 Created`

### Get Products

**GET** `/api/products`

Returns all stored products.

**Response:** `200 OK`

```json
[
  {
    "name": "Keyboard",
    "price": 79.99
  }
]
```

## Local Development

The project uses **Azurite** as a local emulator for Azure Storage.

Make sure Azurite is running before starting the Azure Function.

The local storage connection is configured in `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

Run the project from Visual Studio or with:

```bash
dotnet run
```

The API will be available at the local URL shown when the Azure Function starts.

## Testing with cURL

The examples below assume the Azure Function is running on port `7026`.

### Create a Product

**Windows CMD:**

```cmd
curl -X POST http://localhost:7026/api/products -H "Content-Type: application/json" -d "{\"name\":\"Keyboard\",\"price\":79.99}"
```

Expected response:

```text
201 Created
```

### Get All Products

```cmd
curl http://localhost:7026/api/products
```

Expected response:

```json
[
  {
    "name": "Keyboard",
    "price": 79.99
  }
]
```

### Test Invalid Product Name

```cmd
curl -X POST http://localhost:7026/api/products -H "Content-Type: application/json" -d "{\"name\":\"\",\"price\":79.99}"
```

Expected response:

```text
400 Bad Request
```

### Test Negative Price

```cmd
curl -X POST http://localhost:7026/api/products -H "Content-Type: application/json" -d "{\"name\":\"Keyboard\",\"price\":-10}"
```

Expected response:

```text
400 Bad Request
```

### Test Invalid JSON

```cmd
curl -X POST http://localhost:7026/api/products -H "Content-Type: application/json" -d "{\"name\":\"Keyboard\",\"price\":}"
```

Expected response:

```text
400 Bad Request
```

## Project Structure

```text
ProductApi/
├── Functions/
│   └── ProductFunctions.cs
├── Models/
│   ├── Product.cs
│   └── ProductEntity.cs
├── Services/
│   ├── IProductService.cs
│   └── ProductService.cs
└── Program.cs
```

## Notes

* Dependency injection is used to separate the HTTP Functions from the product storage logic.
* `async/await` is used for Azure Table Storage operations.
* `ILogger` is used for logging warnings and errors.
* Azurite is used for local Azure Storage development and testing.
