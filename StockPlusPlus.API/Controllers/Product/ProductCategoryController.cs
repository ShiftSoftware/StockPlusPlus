using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftSoftware.ShiftEntity.Web;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Shared.ActionTrees;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;

namespace StockPlusPlus.API.Controllers.Product;

[Route("api/[controller]")]
public class ProductCategoryController : ShiftEntitySecureControllerAsync<ProductCategoryRepository, Data.Entities.Product.ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    private readonly ProductCategoryRepository repository;
    public ProductCategoryController(ProductCategoryRepository repository) : base(StockActionTrees.ProductCategory, x =>
        x.FilterBy(x => x.ID, StockActionTrees.DataLevelAccess.ProductCategory)
        .DecodeHashId<_ProductCategoryHashId>()
        .IncludeCreatedByCurrentUser(x => x.CreatedByUserID)
    )
    {
        this.repository = repository;
    }

    [HttpGet("print/{ID}")]
    [AllowAnonymous]
    public async Task<ActionResult> Print(string id)
    {
        return new FileStreamResult(await this.repository.Print(id), "application/pdf");
    }
}
