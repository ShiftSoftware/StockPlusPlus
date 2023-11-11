
using AutoMapper;
using ShiftSoftware.ShiftEntity.EFCore;
using StockPlusPlus.Data.Entities.Product;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;

namespace StockPlusPlus.Data.Repositories.Product;

public class ProductCategoryRepository : ShiftRepository<DB, ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    public ProductCategoryRepository(DB db, IMapper mapper) : base(db, mapper)
    {
    }
}
