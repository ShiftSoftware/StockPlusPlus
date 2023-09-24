using Microsoft.AspNetCore.Mvc;
using ShiftSoftware.ShiftEntity.Web;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Shared.ActionTrees;
using StockPlusPlus.Shared.DTOs.Product.Brand;

namespace StockPlusPlus.API.Controllers.Product;

[Route("api/[controller]")]
public class BrandController : ShiftEntitySecureControllerAsync<BrandRepository, Data.Entities.Product.Brand, BrandListDTO, BrandDTO>
{
    public BrandController() : base(StockActionTrees.Brand, x =>
        x.FilterBy(x => x.ID, StockActionTrees.DataLevelAccess.Brand)
        .IncludeCreatedByCurrentUser(x => x.CreatedByUserID)
    )
    {

    }
}
