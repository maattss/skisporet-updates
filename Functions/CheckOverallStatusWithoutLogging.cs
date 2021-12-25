using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SkisporetUpdates.Helpers;
using SkisporetUpdates.Models;

namespace SkisporetUpdates.Functions
{
    public class CheckOverallStatusWithoutLogging
    {
        [Function("CheckOverallStatusWithoutLogging")]
        [TableOutput("OverallStatus")]
        public static async Task<TrackStatus> RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer,
            [TableInput("OverallStatus", "DestinationKnaben")] IEnumerable<TrackStatus> tableInput,
            FunctionContext context)
        {
            var sortedTableInputs = tableInput.OrderBy(t => t.Timestamp).ToList();
            var previousStatus = sortedTableInputs[^1].PrepStatus;

            if (!(FetchStatus.ShouldFetchNewData())) return null;

            var destinationResponse = await FetchStatus.FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            var mailStatus = "Not sent";
            
            if (currentPrepStatus == 1 && previousStatus != 1)
            {
                var response = Mail.SendTrackUpdateMail();
                mailStatus = response.Result.StatusCode.ToString();

                if (mailStatus == HttpStatusCode.Accepted.ToString() || mailStatus == HttpStatusCode.OK.ToString())
                {
                    mailStatus = $"Sent, status code: {response.Result.StatusCode}";
                }
                else
                {
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