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

        await replication.SetUp<DB, CompanyBranch>(connectionString, databaseId, q => q.Include(x => x.Region).Include(x => x.Company))
            .Replicate<CompanyBranchModel>("CompanyBranches")
            .RunAsync();

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
