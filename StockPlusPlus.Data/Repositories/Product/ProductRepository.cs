
using AutoMapper;
using ShiftSoftware.ShiftEntity.EFCore;
using StockPlusPlus.Shared.DTOs.Product.Product;

namespace StockPlusPlus.Data.Repositories.Product;

public class ProductRepository : ShiftRepository<DB, Entities.Product.Product, ProductListDTO, ProductDTO>
{
    public ProductRepository(DB db, IMapper mapper) : base(db, db.Products, mapper)
    {
    }
}
