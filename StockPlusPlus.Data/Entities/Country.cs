using ShiftSoftware.ShiftEntity.Core;

namespace StockPlusPlus.Data.Entities;

[TemporalShiftEntity]
[ShiftEntityKeyAndName(nameof(ID), nameof(Name))]
public class Country : ShiftEntity<Country>
{
    public string Name { get; set; } = default!;
}
