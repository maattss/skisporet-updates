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
        private static readonly Dictionary<int, string> LineIdToTrackName = new ()
        {
            {24046, "Sanden"},
            {39808, "Husmorloypa"},
            {24039, "Ornehommen"},
            {39801, "Knaben1"},
            {49990, "Knaberoisa"},
            {45635, "Kvinavegen"}
        };
        
        private static readonly Dictionary<string, int> ColorToPrepStatus = new ()
        {
            {"#7a7a7a", 6},
            {"#000005", 5},
            {"#0294ff", 4},
            {"#000003", 3},
            {"#000002", 2},
            {"#000001", 1}
        };

        [Function("CheckTrackStatus")]
        [TableOutput("TrackStatus")]
        public static async Task<List<TrackStatus>> RunAsync([TimerTrigger("* */24 * * * *")] TimerInfo timer,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckTrackStatus");

            if(!(FetchStatus.ShouldFetchNewData(logger))) return null;

            var trackListResponse = await FetchStatus.FetchTrackDataListAsync();
            var trackStatusList = new List<TrackStatus>();

            foreach(var (_, trackData) in trackListResponse)
            {
                LineIdToTrackName.TryGetValue(trackData.LineId, out var trackName);
                ColorToPrepStatus.TryGetValue(trackData.Statuses[0].ColorName, out var prepStatus);
                trackStatusList.Add(new TrackStatus()
                {
                    PartitionKey = trackName ?? "UkjentId",
                    RowKey = Guid.NewGuid().ToString(),
                    PrepStatus = prepStatus == 0 ? 2000 : prepStatus,
                });
            }

            return trackStatusList;
        }
    }
}
