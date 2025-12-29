using SecureInventory.Core.Entities;

namespace SecureInventory.Core.Interfaces;

/// <summary>
/// Defines the contract for product data access operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product if found; otherwise, null.</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product asynchronously.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created product.</returns>
    Task<int> CreateAsync(Product product);
}