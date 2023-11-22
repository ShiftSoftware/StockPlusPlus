
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockPlusPlus.Data;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Functions;
using System;

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
            x.AddAutoMapper(typeof(ShiftSoftware.ShiftIdentity.Data.Marker).Assembly);

        })
            .RegisterShiftEntityEfCoreTriggers()
            .AddDbContext<DB>(options => options.UseSqlServer(configuration.GetConnectionString("SQLServer")))
            .AddScoped<ProductCategoryRepository>()
            .AddScoped<BrandRepository>()
            .AddScoped<ProductRepository>();

        builder.Services.AddShiftEntityCosmosDbReplication();
    }
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
           .SetBasePath(Environment.CurrentDirectory)
           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();
    }
}