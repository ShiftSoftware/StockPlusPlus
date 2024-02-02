using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using StockPlusPlus.Data.Repositories.Product;
using System.Net;
using System.Security.Claims;

namespace StockPlusPlus.Functions
{
    [Authorize]
    public class ProductCategories
    {
        private readonly ProductCategoryRepository productCategoryRepository;
        public ProductCategories(ProductCategoryRepository productCategoryRepository)
        {
            this.productCategoryRepository = productCategoryRepository;
        }

        [Function("ProductCategories")]
        [Authorize(AuthenticationSchemes = "A,S", Roles ="User,Admin")]
        //[AllowAnonymous]
        public async Task<HttpResponseData> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req, FunctionContext functionContext)
        {
            //var identity = functionContext.Items["User"] as ClaimsPrincipal;
            ////var name = identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //var response = req.CreateResponse(HttpStatusCode.OK);
            //await response.WriteStringAsync(identity.Identity.Name);

            //return response;

            var allProductCategories = await this.productCategoryRepository.OdataList().ToArrayAsync();
            
            Data.Entities.Product.ProductCategory? productCategory = null;

            if (allProductCategories.Count() > 0)
            { 
                var productCategoryId = long.Parse(allProductCategories.First().ID);
                productCategory = await this.productCategoryRepository.FindAsync(productCategoryId);
            }

            string responseMessage = System.Text.Json.JsonSerializer.Serialize(new
            {
                AllProducts = allProductCategories,
                FirstProductCategory = productCategory is null ? null : await this.productCategoryRepository.ViewAsync(productCategory)
            });

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                AllProducts = allProductCategories,
                FirstProductCategory = productCategory is null ? null : await this.productCategoryRepository.ViewAsync(productCategory)
            });

            return response;
        }
    }
}
