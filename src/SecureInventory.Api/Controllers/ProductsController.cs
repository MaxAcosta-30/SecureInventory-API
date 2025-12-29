using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;

namespace SecureInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>An action result containing the product if found, otherwise a Not Found response.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound("Producto no encontrado.");
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product data to create.</param>
    /// <returns>An action result indicating the outcome of the creation, with the new product's ID.</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var newId = await _repository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = newId }, product);
    }
}