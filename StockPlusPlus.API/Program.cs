using Microsoft.EntityFrameworkCore;
using ShiftSoftware.ShiftEntity.CosmosDbReplication.Extensions;
using ShiftSoftware.ShiftEntity.EFCore.Extensions;
using ShiftSoftware.ShiftEntity.Web.Extensions;
using ShiftSoftware.ShiftIdentity.AspNetCore.Models;
using ShiftSoftware.ShiftIdentity.AspNetCore;
using ShiftSoftware.ShiftIdentity.Core;
using ShiftSoftware.ShiftIdentity.Core.DTOs;
using StockPlusPlus.Data;
using ShiftSoftware.ShiftIdentity.Dashboard.AspNetCore.Extentsions;
using ShiftSoftware.ShiftIdentity.AspNetCore.Extensions;
using ShiftSoftware.ShiftEntity.Web.Services;
using Microsoft.Extensions.Azure;
using ShiftSoftware.TypeAuth.AspNetCore.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using StockPlusPlus.Data.Repositories.Product;
using StockPlusPlus.Shared.DTOs.Product.ProductCategory;
using StockPlusPlus.Shared.DTOs.Product.Brand;
using StockPlusPlus.Shared.DTOs.Product.Product;
using ShiftSoftware.ShiftEntity.Model;

var builder = WebApplication.CreateBuilder(args);

var fakeUser = new TokenUserDataDTO
{
    FullName = "Fake User",
    ID = "1",
    Username = "fake-user"
};

Action<DbContextOptionsBuilder> dbOptionBuilder = x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")!)
    .UseTemporal(true);
};

if (builder.Configuration.GetValue<bool>("CosmosDb:Enabled"))
{
    builder.Services.AddShiftEntityCosmosDbReplication(x =>
    {
        x.ConnectionString = builder.Configuration.GetValue<string>("CosmosDb:ConnectionString");
        x.DefaultDatabaseName = builder.Configuration.GetValue<string>("CosmosDb:DefaultDatabaseName");
        x.AddShiftDbContext<DB>(dbOptionBuilder);
        x.RepositoriesAssembly= typeof(DB).Assembly;

        x.Accounts.Add(new CosmosDBAccount(builder.Configuration.GetValue<string>("CosmosDb:ConnectionString")!,
            "Identity", false, builder.Configuration.GetValue<string>("CosmosDb:DefaultDatabaseName")));
    });
}

builder.Services
    .AddLocalization()
    .AddHttpContextAccessor()
    .AddDbContext<DB>(dbOptionBuilder)
    .AddControllers()
    .AddShiftEntityWeb(x =>
    {
        x.WrapValidationErrorResponseWithShiftEntityResponse(true);
        x.AddAutoMapper(typeof(StockPlusPlus.Data.Marker).Assembly);

        x.HashId.RegisterHashId(builder.Configuration.GetValue<bool>("Settings:HashIdSettings:AcceptUnencodedIds"));
        x.HashId.RegisterIdentityHashId("one-two", 5);

        var azureStorageAccounts = new List<ShiftSoftware.ShiftEntity.Core.Services.AzureStorageOption>();

        builder.Configuration.Bind("AzureStorageAccounts", azureStorageAccounts);

        x.AddAzureStorage(azureStorageAccounts.ToArray());

        x.AddShiftIdentityAutoMapper();
        x.RepositoriesAssembly = typeof(StockPlusPlus.Data.Marker).Assembly;
    })
    .AddShiftIdentity(builder.Configuration.GetValue<string>("Settings:TokenSettings:Issuer")!, builder.Configuration.GetValue<string>("Settings:TokenSettings:Key")!)
    .AddShiftIdentityDashboard<DB>(
        new ShiftIdentityConfiguration
        {
            ShiftIdentityHostingType = ShiftIdentityHostingTypes.Internal,
            Token = new TokenSettingsModel
            {
                ExpireSeconds = 60000,
                Issuer = builder.Configuration.GetValue<string>("Settings:TokenSettings:Issuer")!,
                Key = builder.Configuration.GetValue<string>("Settings:TokenSettings:Key")!,
            },
            Security = new SecuritySettingsModel
            {
                LockDownInMinutes = 0,
                LoginAttemptsForLockDown = 1000000,
                RequirePasswordChange = false
            },
            RefreshToken = new TokenSettingsModel
            {
                Audience = "stock-plus-plus",
                ExpireSeconds = 60000000,
                Issuer = builder.Configuration.GetValue<string>("Settings:TokenSettings:Issuer")!,
                Key = builder.Configuration.GetValue<string>("Settings:TokenSettings:Key")!,
            },
            HashIdSettings = new HashIdSettings
            {
                AcceptUnencodedIds = true,
                UserIdsSalt = "k02iUHSb2ier9fiui02349AbfJEI",
                UserIdsMinHashLength = 5
            },
        }
    )
    .AddShiftEntityOdata(x =>
    {
        x.DefaultOptions();
        x.OdataEntitySet<BrandListDTO>("Brand");
        x.OdataEntitySet<ProductCategoryListDTO>("ProductCategory");
        x.OdataEntitySet<ProductListDTO>("Product");
        x.RegisterShiftIdentityDashboardEntitySets();
    });
//.AddFakeIdentityEndPoints(
//    new TokenSettingsModel
//    {
//        Issuer = "ToDo",
//        Key = "one-two-three-four-five-six-seven-eight.one-two-three-four-five-six-seven-eight",
//        ExpireSeconds = 60
//    },
//    fakeUser,
//    new ShiftSoftware.ShiftIdentity.Core.DTOs.App.AppDTO
//    {
//        AppId = "to-do-dev",
//        DisplayName = "ToDo Dev",
//        RedirectUri = "http://localhost:5028/Auth/Token"
//    },
//    "123a",
//    new string[] {
//        """
//            {
//                "ToDoActions": [1,2,3,4]
//            }
//        """
//    }
//);
builder.Services.AddSwaggerGen(c =>
{
    c.DocInclusionPredicate(SwaggerService.DocInclusionPredicate);
});

builder.Services.AddScoped<BrandRepository>();
builder.Services.AddScoped<ProductCategoryRepository>();
builder.Services.AddScoped<ProductRepository>();

builder.Services.AddTypeAuth((o) =>
{
    o.AddActionTree<ShiftIdentityActions>();
    o.AddActionTree<StockPlusPlus.Shared.ActionTrees.SystemActionTrees>();
    o.AddActionTree<StockPlusPlus.Shared.ActionTrees.StockActionTrees>();
});

#if DEBUG
builder.Services.AddRazorPages();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnectionString:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnectionString:queue"]!, preferMsi: true);
});
#endif

var app = builder.Build();

//app.AddFakeIdentityEndPoints();

if (app.Environment.EnvironmentName != "Test")
{
    await app.SeedDBAsync("OneTwo", new ShiftSoftware.ShiftIdentity.Data.DBSeedOptions
    {
        RegionExternalId = "1",
        RegionShortCode = "KRG",

        CompanyShortCode = "SFT",
        CompanyExternalId = "-1",
        CompanyAlternativeExternalId = "shift-software",
        CompanyType = ShiftSoftware.ShiftIdentity.Core.Enums.CompanyTypes.NotSpecified,

        CompanyBranchExternalId = "-11",
        CompanyBranchShortCode = "SFT-EBL"
    });

}

var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("en-US"),
    new CultureInfo("ar-IQ"),
    new CultureInfo("ku-IQ"),
};

app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider> { new AcceptLanguageHeaderRequestCultureProvider() };
    options.ApplyCurrentCultureToResponseHeaders = true;
});

#if DEBUG

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

#endif

app.MapControllers();

app.UseCors(x => x.WithOrigins("*").AllowAnyMethod().AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#if DEBUG

app.MapRazorPages();
app.MapFallbackToFile("index.html");

#endif

app.Run();