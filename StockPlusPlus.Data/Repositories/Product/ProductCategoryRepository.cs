
using AutoMapper;
using ShiftSoftware.ShiftEntity.EFCore;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;

namespace StockPlusPlus.Data.Repositories.Product;

public class ProductCategoryRepository : ShiftRepository<DB, Entities.Product.ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    public ProductCategoryRepository(DB db, IMapper mapper) : base(db, db.ProductCategories, mapper)
    {
    }
}
