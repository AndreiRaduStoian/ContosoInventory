using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;

namespace ContosoInventory.Server.Repositories;

/// <summary>
/// Provides EF Core persistence operations for inventory products.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly InventoryContext _context;

    /// <summary>
    /// Initializes the product repository.
    /// </summary>
    public ProductRepository(InventoryContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetProductsAsync(int? categoryId)
    {
        var query = _context.Products.AsNoTracking();
        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        return await query.OrderBy(product => product.Name).ToListAsync();
    }

    /// <inheritdoc />
    public Task<Product?> GetProductByIdAsync(int id, bool asNoTracking = false)
    {
        var query = _context.Products.AsQueryable();
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(product => product.Id == id);
    }

    /// <inheritdoc />
    public Task<bool> CategoryExistsAsync(int categoryId)
    {
        return _context.Categories.AnyAsync(category => category.Id == categoryId);
    }

    /// <inheritdoc />
    public Task<bool> SkuExistsAsync(string sku, int? excludingProductId = null)
    {
        var query = _context.Products.Where(product => product.Sku == sku);
        if (excludingProductId.HasValue)
        {
            query = query.Where(product => product.Id != excludingProductId.Value);
        }

        return query.AnyAsync();
    }

    /// <inheritdoc />
    public Task<bool> ProductExistsAsync(int id)
    {
        return _context.Products.AnyAsync(product => product.Id == id);
    }

    /// <inheritdoc />
    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    /// <inheritdoc />
    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> TryRestockAsync(int id, int quantity, DateTime updatedDate)
    {
        var updatedRows = await _context.Products
            .Where(product => product.Id == id && product.StockQuantity <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(product => product.StockQuantity, product => product.StockQuantity + quantity)
                .SetProperty(product => product.LastUpdatedDate, updatedDate));

        return updatedRows > 0;
    }
}