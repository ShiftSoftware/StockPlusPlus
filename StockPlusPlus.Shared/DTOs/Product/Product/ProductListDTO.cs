
using ShiftSoftware.ShiftEntity.Model.Dtos;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;
using StockPlusPlus.Shared.Enums.Product;
using System.Text.Json.Serialization;

namespace StockPlusPlus.Shared.DTOs.Product.Product;

public class ProductListDTO : ShiftEntityListDTO
{
    [_ProductCategoryHashId]
    public override string? ID { get; set; }
    public string Name { get; set; } = default!;

    public string? Brand { get; set; }
    public string? Category { get; set; }

    [_ProductCategoryHashId]
    public string? ProductCategoryID { get; set; }

    public string? BrandID { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrackingMethod TrackingMethod { get; set; }
}
