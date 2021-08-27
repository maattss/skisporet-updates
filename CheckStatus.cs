using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RestSharp;
using SkisporetUpdates.Models;

namespace SkisporetUpdates
{
    public static class CheckStatus
    {
        [Function("CheckStatus")]
        public static async Task RunAsync([TimerTrigger("*/10 * * * * *")] FunctionContext context)
        {
            // TODO:
            // Check if between 1. Decemeber and 1. May - Check status for every run
            // If not, only check status from if time is between 12.00.00 - 12.00.10 

            // TODO: Add logging
            //var logger = context.GetLogger("Function1");
            //logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            //logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var destinationResponse = await FetchDestinationDataAsync();
            Console.WriteLine(destinationResponse.Data.Symbols[0].Name);

            var trackResponse = await FetchTrackDataListAsync();
            // Console.WriteLine(trackResponse);

            // TODO: Fetch status from all track

            // TODO: Save status to database

            // TODO: Send mail if status changes (use send grid?)
            // Figure out logic of when to send mail

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

        private static async Task<IRestResponse<List<TrackData>>> FetchTrackDataAsync(string body)
        {
            RestClient client = new("https://skisporet.no/map/client/trackstatusstandalonedraw:standaloneStatusForLines") { Timeout = -1 };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
            return await client.ExecuteAsync<List<TrackData>>(request);
        }

        private static async Task<IDictionary<string, TrackData>> FetchTrackDataListAsync()
        {
            var trackDataList = new Dictionary<string, TrackData>();

            var sandenResponse = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B24046%5D");
            trackDataList.Add("Sanden", sandenResponse.Data[0]);

            var husmorLoypaResponse = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B39808%5D");
            trackDataList.Add("HusmorLoypa", husmorLoypaResponse.Data[0]);

            var ornehommenResponse = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B24039%5D");
            trackDataList.Add("Ornehommen", ornehommenResponse.Data[0]);

            var knaben1Response = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B39801%5D");
            trackDataList.Add("Knaben1", knaben1Response.Data[0]);

            var knaberoisaResponse = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B49990%5D");
            trackDataList.Add("Knaberoisa", knaberoisaResponse.Data[0]);

            var kvinavegenResponse = await FetchTrackDataAsync(@"trackTypeKey=ts2sWithStatuses&ids=%5B45635%5D");
            trackDataList.Add("Kvinavegen", kvinavegenResponse.Data[0]);

            return trackDataList;
        }
    }
}
