﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StockPlusPlus.Shared.Enums.Product;

namespace StockPlusPlus.Data.ReplicationModels;

public class ProductModel
{
    public string ID { get; set; }

    public string Name { get; set; } = default!;

    [JsonConverter(typeof(StringEnumConverter))]
    public TrackingMethod TrackingMethod { get; set; }

    public long ProductCategoryID { get; set; }

    public long BrandID { get; set; }

    public bool IsDeleted { get; set; }
}
