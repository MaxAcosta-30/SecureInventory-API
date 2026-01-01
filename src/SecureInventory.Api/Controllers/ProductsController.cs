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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound("Producto no encontrado.");
        return Ok(product);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var newId = await _repository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = newId }, product);
    }

    // ✅ AQUÍ ES DONDE VA EL MÉTODO HTTP PUT
    // Recibe la petición web y llama al repositorio
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        if (id != product.Id) return BadRequest("El ID de la URL no coincide con el cuerpo.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound("Producto no encontrado.");

        await _repository.UpdateAsync(product);

        return NoContent(); // 204 No Content
    }
}