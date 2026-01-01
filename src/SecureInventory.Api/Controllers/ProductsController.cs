using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;

namespace SecureInventory.Api.Controllers;

/// <summary>
/// Controlador responsable de las operaciones CRUD sobre productos del inventario.
/// Implementa el patrón Cache-Aside con Redis para optimizar las consultas de lectura.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de productos.
    /// </summary>
    /// <param name="repository">Repositorio para operaciones de acceso a datos de productos.</param>
    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// Utiliza el patrón Cache-Aside: primero consulta Redis, si no existe, consulta SQL Server y almacena en caché.
    /// </summary>
    /// <param name="id">Identificador único del producto a consultar.</param>
    /// <returns>
    /// - 200 OK: Producto encontrado. Retorna el objeto Product con sus datos (Id, Name, Price, Stock).
    /// - 404 NotFound: El producto con el ID especificado no existe en la base de datos.
    /// - 500 InternalServerError: Error al consultar la base de datos o el caché Redis.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound("Producto no encontrado.");
        return Ok(product);
    }

    /// <summary>
    /// Crea un nuevo producto en el inventario.
    /// Requiere autenticación JWT válida (atributo [Authorize]).
    /// </summary>
    /// <param name="product">Objeto Product con los datos del producto a crear (Name, Price, Stock). El Id será generado automáticamente.</param>
    /// <returns>
    /// - 201 Created: Producto creado exitosamente. Retorna el producto creado con su ID asignado y la ubicación del recurso en el header Location.
    /// - 400 BadRequest: Los datos del producto son inválidos (por ejemplo, precio o stock negativos).
    /// - 401 Unauthorized: No se proporcionó un token JWT válido o el token ha expirado.
    /// - 500 InternalServerError: Error al insertar el producto en la base de datos.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var newId = await _repository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = newId }, product);
    }
}