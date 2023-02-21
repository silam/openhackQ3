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
using System.Collections.Generic;

namespace GetProductFunc
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
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
                string userId = req.Query["userId"];
                HttpClient client = new HttpClient();
                try
                {
                    
                    using HttpResponseMessage response = await client.GetAsync(newconfiguration["GetUserUrl"] + $"?userId={userId}");
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    return new NotFoundObjectResult("User Not Found");
                }
                // TODO: Insert into Cosmos DB
                using CosmosClient dbClient = new CosmosClient(newconfiguration.GetConnectionString("CosmosDBConnectionString"));
                Database database = dbClient.GetDatabase(id: "productdb");
                Container container = database.GetContainer(id: "rating");
                var query = new QueryDefinition(query: "SELECT * FROM ratings r WHERE r.userId = @userId").WithParameter("@userId", userId);
                using FeedIterator<Rating> feed = container.GetItemQueryIterator<Rating>(queryDefinition: query);
                List<Rating> list = new List<Rating>();
                while (feed.HasMoreResults)
                {
                    FeedResponse<Rating> response = await feed.ReadNextAsync();
                    list.AddRange(response);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(list));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.Message);
                return null;
            }
        }
    }
}