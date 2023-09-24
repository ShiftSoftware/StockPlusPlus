using AutoMapper;
using ShiftSoftware.ShiftEntity.EFCore;
using StockPlusPlus.Shared.DTOs.Product.Brand;

namespace StockPlusPlus.Data.Repositories.Product;

public class BrandRepository : ShiftRepository<DB, Entities.Product.Brand, BrandListDTO, BrandDTO>
{
    public BrandRepository(DB db, IMapper mapper) : base(db, db.Brands, mapper)
    {
    }
}
