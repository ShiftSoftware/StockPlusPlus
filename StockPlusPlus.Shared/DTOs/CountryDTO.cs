using ShiftSoftware.ShiftEntity.Model.Dtos;

namespace StockPlusPlus.Shared.DTOs;

public class CountryDTO : ShiftEntityMixedDTO
{
    public override string? ID { get; set; }
    public string Name { get; set; } = default!;
}
