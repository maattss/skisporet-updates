using System;
using System.Collections.Generic;
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
        public static async Task RunAsync([TimerTrigger("*/10 * * * * *")] FunctionContext context)
        {
            // TODO: Update ChronJob expression to "* */10 * * * *"
            var logger = context.GetLogger("CheckStatus");

            /*var start = new DateTime(2021, 12, 01);
            var end = new DateTime(2022, 05, 01);
            if (!(DateTime.Now > start && DateTime.Now < end))
            {
                if (DateTime.Now.Hour == 12)
                {
                    logger.LogInformation($"Daily off season update at: {DateTime.Now}");
                } 
                else
                {
                    logger.LogInformation($"Season not started, returning...");
                    return null;
                }
            }*/
            
            var destinationResponse = await FetchDestinationDataAsync();
            var currentPrepStatus = destinationResponse.Data.Symbols[0].PrepStatus;
            logger.LogInformation("Current prep status: ", currentPrepStatus);

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            logger.LogInformation("API key: " + apiKey);
            var response = SendMail();
            logger.LogInformation("Mail response body: " + response.Result.Body);
            logger.LogInformation("Mail response statuscode: " + response.Result.StatusCode);

            /*if (status.PrepStatus != 1 && currentPrepStatus == 1)
            {
                // SendMail
            }*/

            // var trackResponse = await FetchTrackDataListAsync();
            // logger.LogInformation("Track response", trackResponse);

            // TODO: Save status to database

            // TODO: Send mail if status changes (use send grid?)
            // Figure out logic of when to send mail

            //return new TrackStatus { PartitionKey = "KnabenDestination", RowKey = Guid.NewGuid().ToString(), PrepStatus = destinationResponse.Data.Symbols[0].PrepStatus };

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

        public static async Task<Response> SendMail()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "Example User");
            var subject = "Sending with Twilio SendGrid is Fun";
            var to = new EmailAddress("ma.tyldum@gmail.com", "Mats");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}
