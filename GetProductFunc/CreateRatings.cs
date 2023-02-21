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
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var newconfiguration = new ConfigurationBuilder()
                                    .SetBasePath(context.FunctionAppDirectory)
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .Build();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Guid productId = data?.productId;
            Guid userId = data?.userId;
            string locationName = data?.locationName;
            int rating = Convert.ToInt32(data?.rating);
            string userNotes = data?.userNotes;
            if (rating < 0 || rating > 5)
            {
                return new BadRequestObjectResult("Rating must be between 0 and 5");
            }
            // TODO: Validate userId and productId exists
            HttpClient client = new HttpClient();
            try
            {
                using HttpResponseMessage response = await client.GetAsync(newconfiguration["GetProductUrl"] + $"?productId={productId}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return new NotFoundObjectResult("Product Not Found");
            }
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
            Rating r = new(
                id: Guid.NewGuid(),
                timestamp: DateTime.UtcNow,
                productid: productId,
                userId: userId,
                locationName: locationName,
                rating: rating,
                userNotes: userNotes
            );
            Rating item = await container.CreateItemAsync<Rating>(
                item: r,
                partitionKey: new PartitionKey(r.productid.ToString())
            );
            return new OkObjectResult(JsonConvert.SerializeObject(item));
        }
    }
}