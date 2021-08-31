using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using SkisporetUpdates.Models;

namespace SkisporetUpdates
{
    public static class CheckStatus
    {
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
                if (DateTime.Now.Hour == 12 && DateTime.Now.Minute == 0)
                {
                    logger.LogInformation($"Daily off season update at: {DateTime.Now}");
                }
                else
                {
                    logger.LogInformation($"Season not started, returning...");
                    return null;
                }
            }

            var destinationResponse = await FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            logger.LogInformation("Current prep status: " + currentPrepStatus);
            logger.LogInformation("Previous prep status: " + previousStatus);
            
            if (currentPrepStatus == 1 && previousStatus != 1)
            {
                var response = SendTrackUpdateMail();
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

        public static async Task<Response> SendTrackUpdateMail()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("knaben.loypekjoring@gmail.com", "Knaben Loypekjoring");
            var to = new EmailAddress("ma.tyldum@gmail.com", "Mats");
            var subject = "Skisporet oppdatering!";
            var plainTextContent = "Woho! Løypemaskinen er i gang. Sjekk status på skisporet.no";
            var htmlContent = "<p>Woho! Løypemaskinen er i gang. Sjekk status på <a href=\"https://skisporet.no/setView/58.6657916/7.0643317/15/norges_grunnkart\">skisporet.no</a></p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }

        public class TrackStatus
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public int PrepStatus { get; set; }
        }
    }
}
