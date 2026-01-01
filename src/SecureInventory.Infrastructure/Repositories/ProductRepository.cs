using System.Data;
using System.Text.Json;
using Dapper;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;
using StackExchange.Redis;

namespace SecureInventory.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de productos utilizando Dapper para SQL Server y Redis para caché.
/// Implementa el patrón Cache-Aside para optimizar las consultas de lectura.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly IDatabase _redisDb;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de productos.
    /// </summary>
    /// <param name="dbConnection">Conexión a la base de datos SQL Server inyectada por dependencias.</param>
    /// <param name="redis">Conexión multiplexor a Redis para operaciones de caché.</param>
    public ProductRepository(IDbConnection dbConnection, IConnectionMultiplexer redis)
    {
        _dbConnection = dbConnection;
        _redisDb = redis.GetDatabase();
    }

    /// <summary>
    /// Obtiene un producto por su identificador único de forma asíncrona, utilizando el patrón Cache-Aside.
    /// 
    /// Flujo Cache-Aside:
    /// 1. Consulta Redis con la clave "product:{id}"
    /// 2. Si existe (CACHE HIT): Deserializa y retorna el producto desde Redis
    /// 3. Si no existe (CACHE MISS): Consulta SQL Server, almacena en Redis con TTL de 10 minutos y retorna el producto
    /// 
    /// Esto mejora significativamente el rendimiento al reducir las consultas a la base de datos.
    /// </summary>
    /// <param name="id">Identificador único del producto a recuperar.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado contiene el producto si se encuentra; de lo contrario, null.
    /// </returns>
    /// <remarks>
    /// TTL (Time To Live) del caché: 10 minutos. Después de este tiempo, el producto se elimina
    /// automáticamente de Redis y se debe consultar nuevamente desde SQL Server.
    /// </remarks>
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
    /// Crea un nuevo producto en la base de datos de forma asíncrona.
    /// Inserta el producto y retorna el ID generado automáticamente por SQL Server (IDENTITY).
    /// </summary>
    /// <param name="product">Producto a crear. Debe contener Name, Price y Stock. El Id será generado automáticamente.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado contiene el ID del producto recién creado.
    /// </returns>
    /// <remarks>
    /// NOTA: Este método no actualiza el caché de Redis. Si se requiere consistencia inmediata,
    /// se podría implementar invalidación de caché después de la inserción.
    /// </remarks>
    public async Task<int> CreateAsync(Product product)
    {
        var sql = @"
            INSERT INTO Products (Name, Price, Stock) 
            VALUES (@Name, @Price, @Stock);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await _dbConnection.ExecuteScalarAsync<int>(sql, product);
    }
}