using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Azure.Cosmos;

namespace GetProductFunc
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                var newconfiguration = new ConfigurationBuilder()
                                        .SetBasePath(context.FunctionAppDirectory)
                                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                        .AddEnvironmentVariables()
                                        .Build();


                string ratingId = req.Query["ratingId"];

                var connstr = "AccountEndpoint=https://openhackteam6.documents.azure.com:443/;AccountKey=C2eAWV01Ihh2ikO5EqrMTXtJfe4XSa9Ztr2BIhdUMjQmcaJ6Mq2aCFFccZxRyNuc6ijo2fPQ0GzgACDb1wh7rQ==;";


                // TODO: Insert into Cosmos DB
                using CosmosClient dbClient = new CosmosClient(connstr);
                Database database = dbClient.GetDatabase(id: "productdb");
                Container container = database.GetContainer(id: "rating");

                Rating readItem = await container.ReadItemAsync<Rating>(
                        id: ratingId,
                        partitionKey: new PartitionKey("4c25613a-a3c2-4ef3-8e02-9c335eb23204")
                    );

                return new OkObjectResult(JsonConvert.SerializeObject(readItem));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.Message);
                return null;
            }
        }
    }
}