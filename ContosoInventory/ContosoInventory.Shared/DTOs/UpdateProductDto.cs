using ContosoInventory.Shared.Validation;

namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Supplies the editable values for an existing product.
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    [DecimalPlaces(2)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}