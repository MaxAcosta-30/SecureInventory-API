using SecureInventory.Core.Entities;

namespace SecureInventory.Core.Interfaces;

/// <summary>
/// Define el contrato para las operaciones de acceso a datos de productos.
/// Implementa el patrón Repository para abstraer la lógica de persistencia y caché.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Obtiene un producto por su identificador único de forma asíncrona.
    /// La implementación debe seguir el patrón Cache-Aside: primero consulta Redis, 
    /// si no existe, consulta SQL Server y almacena en caché para futuras consultas.
    /// </summary>
    /// <param name="id">Identificador único del producto a recuperar.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado de la tarea contiene el producto si se encuentra; de lo contrario, null.
    /// </returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Crea un nuevo producto en la base de datos de forma asíncrona.
    /// </summary>
    /// <param name="product">Producto a crear. Debe contener Name, Price y Stock. El Id será generado automáticamente.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado de la tarea contiene el ID del producto recién creado.
    /// </returns>
    Task<int> CreateAsync(Product product);
}