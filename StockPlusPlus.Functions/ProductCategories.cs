using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StockPlusPlus.Data.Repositories.Product;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace StockPlusPlus.Functions
{
    public class ProductCategories
    {
        private readonly ProductCategoryRepository productCategoryRepository;
        public ProductCategories(ProductCategoryRepository productCategoryRepository)
        {
            this.productCategoryRepository = productCategoryRepository;
        }

        [FunctionName("ProductCategories")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
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

            return new OkObjectResult(responseMessage);
        }
    }
}
