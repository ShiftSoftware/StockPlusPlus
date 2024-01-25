using Microsoft.AspNetCore.Mvc;
using ShiftSoftware.ShiftEntity.Web;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Shared.ActionTrees;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;

namespace StockPlusPlus.API.Controllers.Product;

[Route("api/[controller]")]
public class ProductCategoryController : ShiftEntitySecureControllerAsync<ProductCategoryRepository, Data.Entities.Product.ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    public ProductCategoryController() : base(StockActionTrees.ProductCategory, x =>
        x.FilterBy(x => x.ID, StockActionTrees.DataLevelAccess.ProductCategory)
        .DecodeHashId<_ProductCategoryHashId>()
        .IncludeCreatedByCurrentUser(x => x.CreatedByUserID)
    )
    {

    }
}
