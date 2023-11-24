using Microsoft.EntityFrameworkCore;
using ShiftSoftware.ShiftIdentity.AspNetCore.Models;
using ShiftSoftware.ShiftIdentity.AspNetCore;
using ShiftSoftware.ShiftIdentity.Core;
using StockPlusPlus.Data;
using ShiftSoftware.ShiftIdentity.Dashboard.AspNetCore.Extentsions;
using ShiftSoftware.ShiftIdentity.AspNetCore.Extensions;
using ShiftSoftware.ShiftEntity.Web.Services;
using Microsoft.Extensions.Azure;
using ShiftSoftware.TypeAuth.AspNetCore.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

Action<DbContextOptionsBuilder> dbOptionBuilder = x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")!)
    .UseTemporal(true);
};

builder.Services.RegisterShiftRepositories(typeof(StockPlusPlus.Data.Marker).Assembly);

builder.Services.AddDbContext<DB>(dbOptionBuilder);

if (builder.Configuration.GetValue<bool>("CosmosDb:Enabled"))
{
    builder.Services.AddShiftEntityCosmosDbReplicationTrigger(x =>
    {
        //x.ConnectionString = builder.Configuration.GetValue<string>("CosmosDb:ConnectionString");
        //x.DefaultDatabaseName = builder.Configuration.GetValue<string>("CosmosDb:DefaultDatabaseName");
        x.AddShiftDbContext<DB>(dbOptionBuilder);

        //x.Accounts.Add(new CosmosDBAccount(builder.Configuration.GetValue<string>("CosmosDb:ConnectionString")!,
        //    "Identity", false, builder.Configuration.GetValue<string>("CosmosDb:DefaultDatabaseName")));
    });
}

var mvcBuilder = builder.Services
    .AddLocalization()
    .AddHttpContextAccessor()
    .AddControllers();

mvcBuilder.AddShiftEntityWeb(x =>
{
    x.WrapValidationErrorResponseWithShiftEntityResponse(true);
    x.AddAutoMapper(typeof(StockPlusPlus.Data.Marker).Assembly);

    x.HashId.RegisterHashId(builder.Configuration.GetValue<bool>("Settings:HashIdSettings:AcceptUnencodedIds"));
    x.HashId.RegisterIdentityHashId("one-two", 5);

    var azureStorageAccounts = new List<ShiftSoftware.ShiftEntity.Core.Services.AzureStorageOption>();

    builder.Configuration.Bind("AzureStorageAccounts", azureStorageAccounts);

    x.AddAzureStorage(azureStorageAccounts.ToArray());

    x.AddShiftIdentityAutoMapper();
});

mvcBuilder.AddShiftIdentity(builder.Configuration.GetValue<string>("Settings:TokenSettings:Issuer")!, builder.Configuration.GetValue<string>("Settings:TokenSettings:Key")!);

mvcBuilder.AddShiftIdentityDashboard<DB>(
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
);

mvcBuilder.AddShiftEntityOdata(x =>
{
    x.DefaultOptions();
    x.RegisterAllDTOs(typeof(StockPlusPlus.Shared.Marker).Assembly);
    x.RegisterShiftIdentityDashboardEntitySets();
});

builder.Services.AddSwaggerGen(c =>
{
    c.DocInclusionPredicate(SwaggerService.DocInclusionPredicate);
});

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