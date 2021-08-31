using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using SkisporetUpdates.Models;

namespace SkisporetUpdates.Helpers
{
    public class FetchStatus
    {
        public static async Task<IRestResponse<DestinationData>> FetchDestinationDataAsync()
        {
            RestClient client = new("https://skisporet.no/map/client/destinationdraw:DestinationSymbols") { Timeout = -1 };
            RestRequest request = new(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            string body = @"ids=%5B486%5D";
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
            return await client.ExecuteAsync<DestinationData>(request);
        }

        public static async Task<IDictionary<string, TrackData>> FetchTrackDataListAsync()
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

        private static async Task<IRestResponse<List<TrackData>>> FetchTrackDataAsync(string body)
        {
            RestClient client = new("https://skisporet.no/map/client/trackstatusstandalonedraw:standaloneStatusForLines") { Timeout = -1 };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
            return await client.ExecuteAsync<List<TrackData>>(request);
        }
    }
}
