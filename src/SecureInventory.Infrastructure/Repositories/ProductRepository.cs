using System.Data;
using System.Text.Json;
using Dapper;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;
using StackExchange.Redis;

namespace SecureInventory.Infrastructure.Repositories;

/// <summary>
/// Provides data access operations for products, including caching with Redis.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly IDatabase _redisDb;

    public ProductRepository(IDbConnection dbConnection, IConnectionMultiplexer redis)
    {
        _dbConnection = dbConnection;
        _redisDb = redis.GetDatabase();
    }

    /// <summary>
    /// Retrieves a product by its unique identifier asynchronously, utilizing Redis cache.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product if found; otherwise, null.</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        string cacheKey = $"product:{id}";

        var cachedProduct = await _redisDb.StringGetAsync(cacheKey);
        if (!cachedProduct.IsNullOrEmpty)
        {
            Console.WriteLine($"🚀 CACHE HIT: Producto {id} recuperado de Redis.");
            return JsonSerializer.Deserialize<Product>(cachedProduct.ToString());
        }

        Console.WriteLine($"🐌 CACHE MISS: Buscando producto {id} en SQL Server...");
        var sql = "SELECT * FROM Products WHERE Id = @Id";
        var product = await _dbConnection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });

        if (product != null)
        {
            var json = JsonSerializer.Serialize(product);
            await _redisDb.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(10));
        }

        return product;
    }

    /// <summary>
    /// Creates a new product asynchronously.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created product.</returns>
    public async Task<int> CreateAsync(Product product)
    {
        var sql = @"
            INSERT INTO Products (Name, Price, Stock) 
            VALUES (@Name, @Price, @Stock);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await _dbConnection.ExecuteScalarAsync<int>(sql, product);
    }
}