
using AutoMapper;
using ShiftSoftware.ShiftEntity.Core.Services;
using ShiftSoftware.ShiftEntity.EFCore;
using StockPlusPlus.Data.Entities.Product;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;

namespace StockPlusPlus.Data.Repositories.Product;

public class ProductCategoryRepository : ShiftRepository<DB, Entities.Product.ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    private readonly AzureStorageService azureStorageService;
    public ProductCategoryRepository(DB db, IMapper mapper, AzureStorageService azureStorageService) : base(db, db.ProductCategories, mapper)
    {
        this.azureStorageService = azureStorageService;
    }

    public override ValueTask<ProductCategoryDTO> ViewAsync(ProductCategory entity)
    {
        var dto = mapper.Map<ProductCategoryDTO>(entity);

        if (dto.Photos != null)
        {
            dto.Photos.ForEach(f =>
            {
                f.Url = azureStorageService.GetSignedURL(f.Blob!);
            });
        }

        return new ValueTask<ProductCategoryDTO>(dto);
    }
}
