using System.Data;
using System.Text.Json;
using Dapper;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;
using StackExchange.Redis;

namespace SecureInventory.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de productos utilizando Dapper para SQL Server y Redis para caché.
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

    public async Task<int> CreateAsync(Product product)
    {
        var sql = @"
            INSERT INTO Products (Name, Price, Stock) 
            VALUES (@Name, @Price, @Stock);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await _dbConnection.ExecuteScalarAsync<int>(sql, product);
    }

    // ✅ ESTE ES EL MÉTODO CORRECTO PARA EL REPOSITORIO
    // Solo lógica de datos: SQL + Redis Delete
    public async Task UpdateAsync(Product product)
    {
        // 1. Actualizar en SQL Server
        var sql = @"
            UPDATE Products 
            SET Name = @Name, 
                Price = @Price, 
                Stock = @Stock 
            WHERE Id = @Id";

        await _dbConnection.ExecuteAsync(sql, product);

        // 2. INVALIDAR CACHÉ (Smart Cache)
        string cacheKey = $"product:{product.Id}";
        await _redisDb.KeyDeleteAsync(cacheKey);

        Console.WriteLine($"♻️ CACHE INVALIDADO: Se eliminó '{cacheKey}' de Redis tras la actualización.");
    }
}