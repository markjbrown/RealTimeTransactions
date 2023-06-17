using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using cosmos_payments_demo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using payments_model;
using payments_model.Model;
using Newtonsoft.Json.Serialization;

namespace cosmos_payments_demo.APIs
{
    public static class CreateMember
    {
        [FunctionName("CreateMember")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%memberContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] IAsyncCollector<Member> collector,
            ILogger log)
        {
            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var member = JsonSerializationHelper.DeserializeItem<Member>(requestBody);
                if (member != null)
                {
                    member.memberId = Guid.NewGuid().ToString();

                    //Post member to Cosmos DB using output binding
                    await collector.AddAsync(member);
                }
                else
                {
                    return new BadRequestErrorMessageResult(
                        "Invalid member record. Please check the fields and try again.");
                }

                //Return order to caller
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                return new BadRequestResult();
            }
        }
    }
}
