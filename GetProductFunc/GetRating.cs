using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace GetProductFunc
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {


                log.LogInformation("C# HTTP trigger function processed a request.");

                string ratingId = req.Query["ratingId"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                ratingId = ratingId ?? data?.ratingId;

                string responseMessage = string.IsNullOrEmpty(ratingId)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"The product name for your product id {ratingId} is Starfruit Explosion.";


                var AccountEndpoint = "https://openhackteam6.documents.azure.com:443";
                var key = "C2eAWV01Ihh2ikO5EqrMTXtJfe4XSa9Ztr2BIhdUMjQmcaJ6Mq2aCFFccZxRyNuc6ijo2fPQ0GzgACDb1wh7rQ==";

                using CosmosClient client = new(
                                accountEndpoint: AccountEndpoint,
                                authKeyOrResourceToken: key
                );

                // Database reference with creation if it does not already exist
                Database database = client.GetDatabase(id: "productdb");

                Console.WriteLine($"New database:\t{database.Id}");


                // Container reference with creation if it does not alredy exist
                Container container = await database.CreateContainerIfNotExistsAsync(
                    id: "rating",
                    partitionKeyPath: "/productid",
                    throughput: 400
                );


                // Point read item from container using the id and partitionKey
                ProductRating readItem = await container.ReadItemAsync<ProductRating>(
                    id: "79c2779e-dd2e-43e8-803d-ecbebed8972c",
                    partitionKey: new PartitionKey("4c25613a-a3c2-4ef3-8e02-9c335eb23204")
                );


                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.Message);
                return null;
            }
        }


        //C2eAWV01Ihh2ikO5EqrMTXtJfe4XSa9Ztr2BIhdUMjQmcaJ6Mq2aCFFccZxRyNuc6ijo2fPQ0GzgACDb1wh7rQ==


        //        $env:COSMOS_ENDPOINT = "<cosmos-account-URI>"
        //$env:COSMOS_KEY = "<cosmos-account-PRIMARY-KEY>"


        // New instance of CosmosClient class
        
    }
}
