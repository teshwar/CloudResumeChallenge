using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace visitCount
{
    // Document schema matching your Cosmos DB counter
    public class MyDocument
    {
        public string id { get; set; }
        public int count { get; set; }
    }

    // Optional: only needed if using CosmosDBOutput
    public class MultiResponse
    {
        [CosmosDBOutput(
            databaseName: "visits-db",
            containerName: "visits",
            Connection = "CosmosDbConnectionString",
            CreateIfNotExists = true)]
        public MyDocument Document { get; set; }

        public HttpResponseData HttpResponse { get; set; }
    }

    public class VisitCountFunction
    {
        private readonly ILogger _logger;

        public VisitCountFunction(ILogger<VisitCountFunction> logger)
        {
            _logger = logger;
        }

        [Function("visitCount")]
        public async Task<MultiResponse> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("visitCount");
            logger.LogInformation("Processing visitor count.");

            string connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("CosmosDbConnectionString is missing or empty.");
            }

            var client = new CosmosClient(connectionString);
            var database = await client.CreateDatabaseIfNotExistsAsync("visits-db");
            var container = await database.Database.CreateContainerIfNotExistsAsync("visits", "/id");

            var counterId = "visit_counter";
            MyDocument doc;

            try
            {
                // Read the existing counter document
                var item = await container.Container.ReadItemAsync<MyDocument>(counterId, new PartitionKey(counterId));
                doc = item.Resource;
                doc.count += 1; // increment the counter

                // Save updated document
                await container.Container.ReplaceItemAsync(doc, doc.id, new PartitionKey(doc.id));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create the counter if it doesn't exist
                doc = new MyDocument { id = counterId, count = 1 };
                await container.Container.CreateItemAsync(doc, new PartitionKey(doc.id));
            }

            // Create HTTP response
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(doc));

            // Return both HTTP response and document (for CosmosDBOutput if needed)
            return new MultiResponse
            {
                Document = doc,
                HttpResponse = response
            };
        }
    }
}
