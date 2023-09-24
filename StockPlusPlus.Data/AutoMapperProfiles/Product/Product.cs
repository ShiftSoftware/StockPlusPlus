

using AutoMapper;
using StockPlusPlus.Shared.DTOs.Product.Product;

namespace StockPlusPlus.Data.AutoMapperProfiles.Product;

public class Product : Profile
{
    public Product()
    {
        CreateMap<Entities.Product.Product, ProductDTO>().ReverseMap();
        CreateMap<Entities.Product.Product, ProductListDTO>();
    }
}
