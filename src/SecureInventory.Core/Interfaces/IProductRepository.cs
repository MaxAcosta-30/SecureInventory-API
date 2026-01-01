using SecureInventory.Core.Entities;

namespace SecureInventory.Core.Interfaces;

/// <summary>
/// Define el contrato para las operaciones de acceso a datos de productos.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Obtiene un producto por ID.
    /// </summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    Task<int> CreateAsync(Product product);

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    // 👇 ESTA ES LA LÍNEA QUE TE FALTA 👇
    Task UpdateAsync(Product product);
}