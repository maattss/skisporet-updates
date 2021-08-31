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
    public static class CheckOverallStatus
    {
        [Function("CheckOverallStatus")]
        [TableOutput("OverallStatus")]
        public static async Task<TrackStatus> RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timer,
            [TableInput("OverallStatus", "DestinationKnaben")] List<TrackStatus> tableInput,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckOverallStatus");
            var previousStatus = tableInput[^1].PrepStatus;

            if (!(FetchStatus.ShouldFetchNewData(logger))) return null;

            logger.LogInformation("Fetching new ski track data...");
            var destinationResponse = await FetchStatus.FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            logger.LogInformation("Current prep status: " + currentPrepStatus);
            logger.LogInformation("Previous prep status: " + previousStatus);
            
            if (currentPrepStatus == 1 && previousStatus != 1)
            {
                var response = Mail.SendTrackUpdateMail();
                var mailSentStatus = response.Result.StatusCode;
                logger.LogInformation("Mail statuscode: " + mailSentStatus);

                // Do not write to db if statuscode is wrong
                if (mailSentStatus != HttpStatusCode.Accepted) return null;
            }

            return new TrackStatus()
            {
                PartitionKey = "DestinationKnaben",
                RowKey = Guid.NewGuid().ToString(),
                PrepStatus = currentPrepStatus
            };
        } 
    }
}
