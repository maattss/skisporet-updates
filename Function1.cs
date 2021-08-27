using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace SkisporetUpdates
{
    public static class Function1
    {
        [Function("Function1")]
        public static async Task RunAsync([TimerTrigger("*/10 * * * * *")] MyInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("Function1");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            // Fetch skisporet.no data
            var client = new RestClient("https://skisporet.no/map/client/trackstatusstandalonedraw:standaloneStatusForLines");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            var body = @"trackTypeKey=ts2sWithStatuses&ids=%5B24046%5D";
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            logger.LogInformation(response.Content);
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
