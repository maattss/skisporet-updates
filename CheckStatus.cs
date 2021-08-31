using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkisporetUpdates.Helpers;

namespace SkisporetUpdates
{
    public static class CheckStatus
    {
        public class TrackStatus
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public int PrepStatus { get; set; }
        }

        [Function("CheckStatus")]
        [TableOutput("SkisporetStatus")]
        public static async Task<TrackStatus> RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timer,
            [TableInput("SkisporetStatus", "DestinationKnaben")] List<TrackStatus> tableInput,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckStatus");
            var previousStatus = tableInput[^1].PrepStatus;

            var start = new DateTime(2021, 12, 01);
            var end = new DateTime(2022, 05, 01);
            if (!(DateTime.Now > start && DateTime.Now < end))
            {
                if (DateTime.Now.Hour == 11 && DateTime.Now.Minute == 50) // TODO: Change to 12 and 00
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
            var destinationResponse = await FetchStatus.FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            logger.LogInformation("Current prep status: " + currentPrepStatus);
            logger.LogInformation("Previous prep status: " + previousStatus);
            
            if (currentPrepStatus == 6 && previousStatus != 1) // TODO: Change to 1 and 1
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
