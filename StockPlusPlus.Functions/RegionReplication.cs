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

namespace StockPlusPlus.Functions
{
    public class RegionReplication
    {
        private readonly CosmosDbReplication replication;
        private readonly IConfiguration config;

        public RegionReplication(CosmosDbReplication replication,
            IConfiguration config)
        {
            this.replication = replication;
            this.config = config;
        }

        [FunctionName("RegionReplication")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var connectionString = config.GetValue<string>("CosmosDb:ConnectionString");
            var databaseId = config.GetValue<string>("CosmosDb:DefaultDatabaseName");

            await replication.SetUp<DB, Region>(connectionString, databaseId)
                .Replicate<RegionModel>("Regions")
                .RunAsync();

            return new OkObjectResult("Success");
        }
    }
}
