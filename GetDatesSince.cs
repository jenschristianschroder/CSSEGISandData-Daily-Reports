using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PowerAppsGuy.CSSEGISandData
{
    public static class GetDatesSince
    {
        
        [FunctionName("GetDatesSince")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            List<dateObject> list = new List<dateObject>();                

            log.LogInformation("C# HTTP trigger function processed a request.");

            if(!String.IsNullOrEmpty(req.Query["start"])) {
                DateTime startDate = Convert.ToDateTime(req.Query["start"], System.Globalization.CultureInfo.InvariantCulture);
                int daysSince = (DateTime.Now.AddDays(-1) - startDate).Days;

                for(int i = 0; i <= daysSince; i++) {
                    dateObject d = new dateObject();
                    d.date = startDate.AddDays(i).ToString("MM-dd-yyyy");
                    list.Add(d);
                }
                
            }
            JsonResult jsonResult = new JsonResult(list.ToArray());

            return new OkObjectResult(jsonResult.Value);
        }
    }

    class dateObject {
        public string date { get; set;}
    }
}
