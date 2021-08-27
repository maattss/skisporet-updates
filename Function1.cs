using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RestSharp;
using SkisporetUpdates.Models;

namespace SkisporetUpdates
{
    public static class Function1
    {
        [Function("Function1")]
        public static async Task RunAsync([TimerTrigger("*/10 * * * * *")] FunctionContext context)
        {
            //var logger = context.GetLogger("Function1");
            //logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            //logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var destinationResponse = await FetchDestinationDataAsync();
            Console.WriteLine(destinationResponse.Data.Symbols[0].Name);

            var sandenTrackResponse = await FetchTrackDataAsync();
            Console.WriteLine(sandenTrackResponse.Data[0].LineId);

        }

        private static async Task<IRestResponse<DestinationData>> FetchDestinationDataAsync()
        {
            RestClient client = new("https://skisporet.no/map/client/destinationdraw:DestinationSymbols") { Timeout = -1 };
            RestRequest request = new(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            string body = @"ids=%5B486%5D";
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
            return await client.ExecuteAsync<DestinationData>(request);
        }

        private static async Task<IRestResponse<List<TrackData>>> FetchTrackDataAsync()
        {
            // var sandenBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B24046%5D"; // ID:24046
            // var husmorLoypaBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B39808%5D"; // ID:39808
            // var ornehommenBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B24039%5D"; // ID:24039
            // var knaben1Body = @"trackTypeKey=ts2sWithStatuses&ids=%5B39801%5D"; // ID:39801
            // var knaberoisaBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B49990%5D" // ID:49990
            // var kvinavegenBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B45635%5D" // ID:45635

            RestClient client = new("https://skisporet.no/map/client/trackstatusstandalonedraw:standaloneStatusForLines") { Timeout = -1 };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            var sandenBody = @"trackTypeKey=ts2sWithStatuses&ids=%5B24046%5D";
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", sandenBody, ParameterType.RequestBody);
            return await client.ExecuteAsync<List<TrackData>>(request);
        }
    }
}
