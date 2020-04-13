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
using CsvHelper;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace PowerAppsGuy.CSSGISandData
{
    public static class CSSEGISandData
    {
        [FunctionName("CSSEGISandData")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string filename = req.Query["filename"];
            if (String.IsNullOrEmpty(filename))
                filename = DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy");

            string countryFilter = req.Query["country"];

            JsonResult jsonResult = new JsonResult(null);

            HttpClient client = new HttpClient();

            List<dailyReport> list = new List<dailyReport>();                        
            
            try	{
                // get daily report from John Hopkins CSSE repository
                HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_daily_reports/" + filename + ".csv");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("Country/Region", "Country_Region").Replace("Province/State", "Province_State").Replace("Last Update", "Last_Update");

                using (var csv = new CsvReader(new StringReader(responseBody),System.Globalization.CultureInfo.InvariantCulture))
                {
                    list = csv.GetRecords<dailyReport>().ToList();
                    list.Sort();

                    var grouped = list.GroupBy(x => new { x.Country_Region, x.Province_State }).Select(g => new
                        {
                            Date = filename,
                            CountryRegion = g.Key.Country_Region,
                            ProvinceState = (string)g.Key.Province_State,
                            Name = g.Key,
                            Deaths = (int)g.Sum(x => x.Deaths),
                            Confirmed = (int)g.Sum(x => x.Confirmed),
                            Recovered = (int)g.Sum(x => x.Recovered)
                        });
                        jsonResult = new JsonResult(grouped.ToArray());
                }
            }
            catch (Exception e) {
                log.LogCritical(e.Message);
                return new BadRequestObjectResult(e.Message);
            }

            return new OkObjectResult(jsonResult.Value);
        }
    }
    
    class dailyReport : IEquatable<dailyReport>, IComparable<dailyReport>
    {
        public dailyReport() {
            Confirmed = 0;
            Deaths = 0;
            Recovered = 0;
        }
        public string Province_State { get; set; }
        public string Country_Region { get; set; }
        public string Last_Update {get;set;}
        public int? Confirmed {get;set;}
        public int? Deaths {get;set;}
        public int? Recovered {get;set;}
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            dailyReport dailyReportObj = obj as dailyReport;
            if (dailyReportObj == null) return false;
            else return Equals(dailyReportObj);
        }
        public int SortByNameAscending(string name1, string name2)
        {
            
            return name1.CompareTo(name2);
        }

        // Default comparer for dailyReport type.
        public int CompareTo(dailyReport compareDailyReport)
        {
            // A null value means that this object is greater.
            if (compareDailyReport == null)
                return 1;
                
            else {

            
                int result = this.Country_Region.CompareTo(compareDailyReport.Country_Region);
                if(result == 0) {
                    result = this.Province_State.CompareTo(compareDailyReport.Province_State);
                }
                return result;
            }
        }
        public override int GetHashCode()
        {
            return Country_Region.GetHashCode();
        }
        public bool Equals(dailyReport other)
        {
            if (other == null) return false;
            return (this.Country_Region.Equals(other.Country_Region));
        }
    }

}
