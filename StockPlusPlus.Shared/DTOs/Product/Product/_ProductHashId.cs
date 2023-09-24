
using ShiftSoftware.ShiftEntity.Model.HashId;

namespace StockPlusPlus.Shared.DTOs.Product.Product;

public class _ProductHashId : JsonHashIdConverterAttribute<_ProductHashId>
{
    public _ProductHashId() : base(5) { }
}
