using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkisporetUpdates.Helpers;
using SkisporetUpdates.Models;

namespace SkisporetUpdates.Functions
{
    public static class CheckOverallStatus
    {
        [Function("CheckOverallStatus")]
        [TableOutput("OverallStatus")]
        public static async Task<TrackStatus> RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer,
            [TableInput("OverallStatus", "DestinationKnaben")] List<TrackStatus> tableInput,
            FunctionContext context)
        {
            var logger = context.GetLogger("CheckOverallStatus");
            var sortedTableInputs = tableInput.OrderBy(t => t.Timestamp).ToList();
            var previousStatus = sortedTableInputs[^1].PrepStatus;

            if (!(FetchStatus.ShouldFetchNewData(logger))) return null;

            logger.LogInformation("Fetching new ski track data...");
            var destinationResponse = await FetchStatus.FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            logger.LogInformation("Current prep status: {CurrentPrepStatus}", currentPrepStatus);
            logger.LogInformation("Previous prep status: {PreviousStatus}", previousStatus);
            var mailStatus = "Not sent";
            
            if (currentPrepStatus == 1 && previousStatus != 1)
            {
                logger.LogInformation("Sending mail...");
                var response = Mail.SendTrackUpdateMail();
                mailStatus = response.Result.StatusCode.ToString();

                if (mailStatus == HttpStatusCode.Accepted.ToString() || mailStatus == HttpStatusCode.OK.ToString())
                {
                    logger.LogInformation("Mail successfully sent!");
                    mailStatus = $"Sent, status code: {response.Result.StatusCode}";
                }
                else
                {
                    logger.LogInformation("Something seems to be wrong... Mail status code: {MailStatus}", mailStatus);
                    mailStatus = $"Error, status code: {response.Result.StatusCode}";
                }
            }

            return new OverallStatus()
            {
                PartitionKey = "DestinationKnaben",
                RowKey = Guid.NewGuid().ToString(),
                PrepStatus = currentPrepStatus,
                MailStatus = mailStatus,
            };
        } 
    }
}
