﻿
using ShiftSoftware.ShiftEntity.Model.Dtos;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;
using StockPlusPlus.Shared.Enums.Product;
using System.Text.Json.Serialization;

namespace StockPlusPlus.Shared.DTOs.Product.Product;

public class ProductDTO : ShiftEntityDTO
{
    [_ProductCategoryHashId]
    public override string? ID { get; set; }
    public string Name { get; set; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrackingMethod TrackingMethod { get; set; }
}