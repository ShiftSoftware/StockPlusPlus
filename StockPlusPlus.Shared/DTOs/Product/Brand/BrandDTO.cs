
using ShiftSoftware.ShiftEntity.Model.Dtos;

namespace StockPlusPlus.Shared.DTOs.Product.Brand;

public class BrandDTO : ShiftEntityDTO
{
    public override string? ID { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Code { get; set; }
}
