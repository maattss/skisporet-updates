using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkisporetUpdates.Helpers;
using SkisporetUpdates.Models;

namespace SkisporetUpdates
{
    public static class CheckTrackStatus
    {

        [Function("CheckTrackStatus")]
        [TableOutput("SkisporetStatus")]
        public static async Task<List<TrackStatus>> RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timer,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckTrackStatus");

            var start = new DateTime(2021, 12, 01);
            var end = new DateTime(2022, 05, 01);
            if (!(DateTime.Now > start && DateTime.Now < end))
            {
                if (DateTime.Now.Hour == 12 && DateTime.Now.Minute == 00)
                {
                    logger.LogInformation($"Daily off-season update at: {DateTime.Now}");
                }
                else
                {
                    logger.LogInformation($"Season not started, returning...");
                    return null;
                }
            }

            logger.LogInformation("Fetching new ski track data...");
            var trackListResponse = await FetchStatus.FetchTrackDataListAsync();
            var trackStatusList = new List<TrackStatus>();

            foreach(var track in trackListResponse)
            {
                trackStatusList.Add(new TrackStatus()
                {
                    PartitionKey = "",
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
                // TODO: Add rest of colors here
                _ => 2000,
            };
        }
    }
}
