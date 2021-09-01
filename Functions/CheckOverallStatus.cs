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
            var mailStatus = "Not sent";
            
            if (currentPrepStatus == 1 && previousStatus != 1)
            {
                logger.LogInformation("Sending mail...");
                var response = Mail.SendTrackUpdateMail();
                mailStatus = response.Result.StatusCode.ToString();

                if (mailStatus == HttpStatusCode.Accepted.ToString() || mailStatus == HttpStatusCode.OK.ToString())
                {
                    logger.LogInformation("Mail succesfully sent!");
                    mailStatus = "Sent, statuscode: " + response.Result.StatusCode.ToString();
                }
                else
                {
                    logger.LogInformation("Something seems ro be wrong... Mail statuscode: " + mailStatus);
                    mailStatus = "Error, statuscode: " + response.Result.StatusCode.ToString();
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
