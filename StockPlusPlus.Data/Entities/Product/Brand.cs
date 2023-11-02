
using ShiftSoftware.ShiftEntity.Core;

namespace StockPlusPlus.Data.Entities.Product;

[TemporalShiftEntity]
[ShiftEntityValueText(nameof(ID), nameof(Name))]
public class Brand : ShiftEntity<Brand>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Code { get; set; }
}
