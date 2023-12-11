using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShiftSoftware.ShiftEntity.CosmosDbReplication.Services;
using ShiftSoftware.ShiftIdentity.Core.Entities;
using ShiftSoftware.ShiftIdentity.Core.ReplicationModels;
using StockPlusPlus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockPlusPlus.Functions;

public class Test
{
    private readonly CosmosDBReplication replication;
    private readonly IConfiguration config;
    private readonly IMapper mapper;
    private readonly DB db;

    public Test(CosmosDBReplication replication,
        IConfiguration config,
        IMapper mapper,
        DB db)
    {
        this.replication = replication;
        this.config = config;
        this.mapper = mapper;
        this.db = db;
    }

    [FunctionName("Test")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        var connectionString = config.GetValue<string>("CosmosDb:ConnectionString");
        var databaseId = config.GetValue<string>("CosmosDb:DefaultDatabaseName");

        //await replication.SetUp<DB, Region>(connectionString, databaseId)
        //    .Replicate<RegionModel>("Regions", x => this.mapper.Map<RegionModel>(x))
        //    .RunAsync();

        //await replication.SetUp<DB, CompanyBranch>(connectionString, databaseId, q => q.Include(x => x.Region).Include(x => x.Company))
        //    .Replicate("CompanyBranches", x => this.mapper.Map<CompanyBranchModel>(x))
        //    .RunAsync();

        await replication.SetUp<DB, Service>(connectionString, databaseId)
            .Replicate("Services", x => this.mapper.Map<ServiceModel>(x))
            .UpdateReference<CompanyBranchServiceModel>("CompanyBranches",
            (q, e) =>
            {
                var id = e.ID.ToString();
                return q.Where(x => x.ItemType == "Service" && x.id == id);
            },
            (q, r) => q.Where(x => x.id == r.RowID.ToString() && x.ItemType == "Service"))
            .RunAsync(false, true);

        //await replication.SetUp<DB, CompanyBranchService>(connectionString, databaseId, x=> x.Include(i=> i.Service))
        //    .Replicate("CompanyBranches", x => this.mapper.Map<CompanyBranchServiceModel>(x))
        //    .RunAsync();

        //await replication.SetUp<DB, Company>(connectionString, databaseId)
        //    .Replicate("Companies", x => this.mapper.Map<CompanyModel>(x))
        //    .UpdatePropertyReference<CompanyModel, CompanyBranchModel>("CompanyBranches", x => x.Company,
        //    (q, e) => q.Where(x => x.Company.id == e.ID.ToString()),
        //    (q, r) => q.Where(x => x.Company.id == r.RowID.ToString()))
        //    .RunAsync();

        //var braches = await this.db.CompanyBranches
        //    .ProjectTo<CompanyBranchModel>(this.mapper.ConfigurationProvider)
        //    .ToListAsync();

        //var braches = await Query(q => q.Where(x => x.ID == 13).Select(x=> x.Name));
        //var braches = await Query();

        //return new OkObjectResult(braches);
        return new OkObjectResult("Success");
    }

    private async Task<IEnumerable<TResult>> Query<TResult>(Func<IQueryable<CompanyBranch>,IQueryable<TResult>> query)
    {
        //var braches = await this.db.CompanyBranches
        //    .ProjectTo<CompanyBranchModel>(this.mapper.ConfigurationProvider)
        //    .ToListAsync();

        var q = this.db.CompanyBranches.AsQueryable();

        IQueryable<TResult> q2 = default;

        if (query is not null)
            q2 = query(q);
        
        var braches = await q2
            .ToListAsync();

        return braches;
    }
}
