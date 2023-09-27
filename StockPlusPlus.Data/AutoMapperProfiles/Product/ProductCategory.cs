
using AutoMapper;
using ShiftSoftware.ShiftEntity.Model.Dtos;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;
using System.Text.Json;

namespace StockPlusPlus.Data.AutoMapperProfiles.Product;

public class ProductCategory : Profile
{
    public ProductCategory()
    {
        CreateMap<Entities.Product.ProductCategory, ProductCategoryDTO>()
            .ForMember(
                dest => dest.Photos,
                opt => opt.MapFrom(src => deserialize(src.Photos))
            )
            .ReverseMap().ForMember(
                dest => dest.Photos,
                opt => opt.MapFrom(src => Serialize(src.Photos))
            );

        CreateMap<Entities.Product.ProductCategory, ProductCategoryListDTO>();
    }

    private string? Serialize(List<ShiftFileDTO>? shiftFileDTOs)
    {
        if (shiftFileDTOs == null)
            return null;

        shiftFileDTOs.ForEach(x => x.Url = null);

        return JsonSerializer.Serialize(shiftFileDTOs);
    }

    private List<ShiftFileDTO> deserialize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<ShiftFileDTO>();
        }

        return JsonSerializer.Deserialize<List<ShiftFileDTO>>(text) ?? new List<ShiftFileDTO>();
    }
}
