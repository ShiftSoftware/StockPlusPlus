
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShiftSoftware.ShiftEntity.Core.Extensions;
using StockPlusPlus.Data;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Functions;
using System;
using ShiftSoftware.ShiftEntity.EFCore.Extensions;
using ShiftSoftware.ShiftEntity.Core;

[assembly: FunctionsStartup(typeof(Startup))]
namespace StockPlusPlus.Functions;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        builder.Services
        .AddShiftEntity(x =>
        {
            x.HashId.RegisterHashId(false);

            x.AddAutoMapper(typeof(ShiftSoftware.ShiftEntity.EFCore.AutoMapperProfiles.DefaultMappings).Assembly);

            x.AddAutoMapper(typeof(StockPlusPlus.Data.Marker).Assembly);

        })
            .RegisterShiftEntityEfCoreTriggers()
            .AddDbContext<DB>(options => options.UseSqlServer(configuration.GetConnectionString("SQLServer")))
            .AddScoped<ProductCategoryRepository>()
            .AddScoped<BrandRepository>()
            .AddScoped<ProductRepository>();
    }
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
           .SetBasePath(Environment.CurrentDirectory)
           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();
    }
}