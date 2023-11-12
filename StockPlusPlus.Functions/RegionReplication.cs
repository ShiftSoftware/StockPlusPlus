using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShiftSoftware.ShiftEntity.CosmosDbReplication.Services;
using StockPlusPlus.Data;
using StockPlusPlus.Data.Entities.Product;
using ShiftSoftware.ShiftIdentity.Core.Entities;
using Microsoft.Extensions.Configuration;
using ShiftSoftware.ShiftIdentity.Core.ReplicationModels;
using AutoMapper;

namespace StockPlusPlus.Functions
{
    public class RegionReplication
    {
        private readonly CosmosDBReplication replication;
        private readonly IConfiguration config;
        private readonly IMapper mapper;

        public RegionReplication(CosmosDBReplication replication,
            IConfiguration config,
            IMapper mapper)
        {
            this.replication = replication;
            this.config = config;
            this.mapper = mapper;
        }

        [FunctionName("RegionReplication")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var connectionString = config.GetValue<string>("CosmosDb:ConnectionString");
            var databaseId = config.GetValue<string>("CosmosDb:DefaultDatabaseName");

            await replication.SetUp<DB, Region>(connectionString, databaseId)
                .Replicate<RegionModel>("Regions", x => this.mapper.Map<RegionModel>(x))
                .RunAsync();

            return new OkObjectResult("Success");
        }
    }
}
