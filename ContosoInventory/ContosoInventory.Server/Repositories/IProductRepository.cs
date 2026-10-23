using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Defines persistence operations for inventory products.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves products, optionally filtered by category.
    /// </summary>
    Task<List<Product>> GetProductsAsync(int? categoryId);

    /// <summary>
    /// Retrieves a product by identifier.
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id, bool asNoTracking = false);

    /// <summary>
    /// Determines whether a category exists.
    /// </summary>
    Task<bool> CategoryExistsAsync(int categoryId);

    /// <summary>
    /// Determines whether a SKU is used by another product.
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, int? excludingProductId = null);

    /// <summary>
    /// Determines whether a product exists.
    /// </summary>
    Task<bool> ProductExistsAsync(int id);

    /// <summary>
    /// Adds a product to the unit of work.
    /// </summary>
    void Add(Product product);

    /// <summary>
    /// Removes a product from the unit of work.
    /// </summary>
    void Remove(Product product);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Atomically increases stock if the resulting quantity fits in an integer.
    /// </summary>
    Task<bool> TryRestockAsync(int id, int quantity, DateTime updatedDate);
}