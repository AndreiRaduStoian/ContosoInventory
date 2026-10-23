using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Defines operations for managing inventory products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves products, optionally filtered by category.
    /// </summary>
    Task<List<ProductResponseDto>> GetProductsAsync(int? categoryId);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    Task<ProductResponseDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a product.
    /// </summary>
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);

    /// <summary>
    /// Updates a product's details without changing its stock quantity.
    /// </summary>
    Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto);

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Increases a product's stock quantity.
    /// </summary>
    Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto);
}