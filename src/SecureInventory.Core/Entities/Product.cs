namespace SecureInventory.Core.Entities;

/// <summary>
/// Representa un producto del inventario en el sistema SecureInventory.
/// Contiene la información básica de un producto: nombre, precio y stock disponible.
/// </summary>
public class Product
{
    /// <summary>
    /// Identificador único del producto en la base de datos.
    /// Se genera automáticamente mediante IDENTITY en SQL Server.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre descriptivo del producto.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Precio unitario del producto en formato decimal.
    /// Debe ser mayor o igual a cero (validado a nivel de base de datos).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cantidad de unidades disponibles en inventario.
    /// Debe ser mayor o igual a cero (validado a nivel de base de datos).
    /// </summary>
    public int Stock { get; set; }
}