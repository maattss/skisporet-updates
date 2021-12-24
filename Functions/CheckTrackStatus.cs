using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SkisporetUpdates.Helpers;
using SkisporetUpdates.Models;

namespace SkisporetUpdates.Functions
{
    public static class CheckTrackStatus
    {

        [Function("CheckTrackStatus")]
        [TableOutput("TrackStatus")]
        public static async Task<List<TrackStatus>> RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckTrackStatus");

            if(!(FetchStatus.ShouldFetchNewData(logger))) return null;

            var trackListResponse = await FetchStatus.FetchTrackDataListAsync();
            var trackStatusList = new List<TrackStatus>();

            foreach(var track in trackListResponse)
            {
                trackStatusList.Add(new TrackStatus()
                {
                    PartitionKey = FetchStatus.LineIdToTrackName(track.Value.LineId),
                    RowKey = Guid.NewGuid().ToString(),
                    PrepStatus = ColorToPrepStatus(track.Value.Statuses[0].ColorName),
                });
            }

            return trackStatusList;
        }

        private static int ColorToPrepStatus(string hexColor)
        {
            return hexColor switch
            {
                "#7a7a7a" => 6,
                "#000005" => 5,
                "#0294ff" => 4,
                "#000003" => 3,
                "#000002" => 2,
                "#000001" => 1,
                _ => 2000,
            };
        }
 
    }
}
