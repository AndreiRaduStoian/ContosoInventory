using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Controllers;

/// <summary>
/// Manages inventory product operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a products controller.
    /// </summary>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all products, optionally filtered by category.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] int? categoryId)
    {
        _logger.LogInformation("Retrieving products for category {CategoryId}.", categoryId);
        return Ok(await _productService.GetProductsAsync(categoryId));
    }

    /// <summary>
    /// Returns a product by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        _logger.LogInformation("Retrieving product with ID {ProductId}.", id);
        var product = await _productService.GetProductByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Creates a product.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating product with SKU {ProductSku}.", dto.Sku);
        try
        {
            var product = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a product's details.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with ID {ProductId}.", id);
        try
        {
            var product = await _productService.UpdateProductAsync(id, dto);
            return product == null ? NotFound() : Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}.", id);
        return await _productService.DeleteProductAsync(id) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Increases a product's stock quantity.
    /// </summary>
    [HttpPost("{id}/restock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestockProduct([FromRoute] int id, [FromBody] RestockProductDto dto)
    {
        _logger.LogInformation("Restocking product with ID {ProductId} by {Quantity} units.", id, dto.Quantity);
        try
        {
            var product = await _productService.RestockProductAsync(id, dto);
            return product == null ? NotFound() : Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}