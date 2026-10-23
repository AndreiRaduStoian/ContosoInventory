using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Models;
using ContosoInventory.Server.Repositories;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory products.
/// </summary>
public class ProductService : IProductService
{
    private const int SqliteUniqueConstraintError = 2067;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a product service.
    /// </summary>
    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductResponseDto>> GetProductsAsync(int? categoryId)
    {
        try
        {
            var products = await _productRepository.GetProductsAsync(categoryId);
            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}.", categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id, asNoTracking: true);
            return product == null ? null : MapToDto(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateDto(dto);

        var sku = dto.Sku.Trim();
        try
        {
            await EnsureCategoryExistsAsync(dto.CategoryId);
            if (await _productRepository.SkuExistsAsync(sku))
            {
                throw new InvalidOperationException($"A product with the SKU '{sku}' already exists.");
            }

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = sku,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = now,
                LastUpdatedDate = now
            };

            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync();
            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}).", product.Name, product.Id);
            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex) when (IsSkuUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Product SKU {ProductSku} already exists.", sku);
            throw new InvalidOperationException($"A product with the SKU '{sku}' already exists.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with SKU {ProductSku}.", dto.Sku);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateDto(dto);

        var sku = dto.Sku.Trim();
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            await EnsureCategoryExistsAsync(dto.CategoryId);
            if (await _productRepository.SkuExistsAsync(sku, id))
            {
                throw new InvalidOperationException($"A product with the SKU '{sku}' already exists.");
            }

            product.Name = dto.Name.Trim();
            product.Sku = sku;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync();
            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}).", product.Name, product.Id);
            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex) when (IsSkuUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Product SKU {ProductSku} already exists.", sku);
            throw new InvalidOperationException($"A product with the SKU '{sku}' already exists.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync();
            _logger.LogInformation("Product deleted: {ProductName} (ID: {ProductId}).", product.Name, product.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateDto(dto);

        try
        {
            var updated = await _productRepository.TryRestockAsync(id, dto.Quantity, DateTime.UtcNow);
            if (!updated)
            {
                if (!await _productRepository.ProductExistsAsync(id))
                {
                    return null;
                }

                throw new InvalidOperationException("The restock quantity exceeds the maximum supported stock quantity.");
            }

            var product = await _productRepository.GetProductByIdAsync(id, asNoTracking: true);
            if (product == null)
            {
                return null;
            }

            _logger.LogInformation("Product restocked: {ProductId} by {Quantity} units.", id, dto.Quantity);
            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw;
        }
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await _productRepository.CategoryExistsAsync(categoryId))
        {
            throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");
        }
    }

    private static void ValidateDto(object dto)
    {
        try
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        }
        catch (ValidationException ex)
        {
            throw new ArgumentException(ex.Message, ex);
        }
    }

    private static bool IsSkuUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteExtendedErrorCode: SqliteUniqueConstraintError
        };
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedDate = product.CreatedDate,
            LastUpdatedDate = product.LastUpdatedDate
        };
    }
}