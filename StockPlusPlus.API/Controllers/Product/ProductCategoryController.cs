using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftSoftware.ShiftEntity.Web;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Shared.ActionTrees;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;
using System.Security.Cryptography;
using System.Text;

namespace StockPlusPlus.API.Controllers.Product;

[Route("api/[controller]")]
public class ProductCategoryController : ShiftEntitySecureControllerAsync<ProductCategoryRepository, Data.Entities.Product.ProductCategory, ProductCategoryListDTO, ProductCategoryDTO>
{
    private readonly ProductCategoryRepository repository;
    private readonly IConfiguration configuration;

    public ProductCategoryController(ProductCategoryRepository repository, IConfiguration configuration) : base(StockActionTrees.ProductCategory, x =>
        x.FilterBy(x => x.ID, StockActionTrees.DataLevelAccess.ProductCategory)
        .DecodeHashId<_ProductCategoryHashId>()
        .IncludeCreatedByCurrentUser(x => x.CreatedByUserID)
    )
    {
        this.repository = repository;
        this.configuration = configuration;
    }

    //This endpoint should be protected by bearer token and TypeAuth
    [HttpGet("print-token/{ID}")]
    [AllowAnonymous]
    public ActionResult PrintToken(string id)
    {
        var (token, expires) = this.GeneratePrintToken(typeof(Data.Entities.Product.ProductCategory), id, DateTime.UtcNow.AddMinutes(1));

        return Ok($"expires={expires}&token={token}");
    }

    public (string token, string expires) GeneratePrintToken(Type type, string id, DateTime expirationTime)
    {
        var key = this.configuration.GetValue<string>("Settings:TokenSettings:Key")!;

        var expires = expirationTime.ToString("yyyy-MM-dd.HH-mm-ss-ffff");

        var data = $"{type.FullName}-{id}-{expires}";

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            var token = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return (token, expires);
        }
    }

    [HttpGet("print/{ID}")]
    [AllowAnonymous]
    public async Task<ActionResult> Print(string id, [FromQuery] string expires, [FromQuery] string token)
    {
        if (!ValidatePrintToken(typeof(Data.Entities.Product.ProductCategory), id, expires, token))
            return Forbid();

        return new FileStreamResult(await this.repository.Print(id), "application/pdf");
    }

    private bool ValidatePrintToken(Type type, string id, string expires, string token)
    {
        try
        {
            var expirationTime = DateTime.ParseExact(expires, "yyyy-MM-dd.HH-mm-ss-ffff", System.Globalization.CultureInfo.InvariantCulture);

            if (DateTime.UtcNow > expirationTime)
                return false;

            var newToken = GeneratePrintToken(type, id, expirationTime).token;

            //A simple equality work. But to prevent against timing attack the below is more secure
            //return newToken.Equals(token);

            ReadOnlySpan<byte> newTokenBytes = Convert.FromBase64String(newToken);

            ReadOnlySpan<byte> providedTokenBytes = Convert.FromBase64String(token);

            return CryptographicOperations.FixedTimeEquals(newTokenBytes, providedTokenBytes);
        }
        catch
        {
            return false;
        }
    }
}
