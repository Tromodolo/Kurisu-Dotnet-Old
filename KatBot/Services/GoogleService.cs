using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;

namespace KatBot.Services
{
    public class GoogleService
    {
        public async Task<Search> GoogleSearchAsync(string query)
        {
            var apiKey = Katarina.botData.googleapikey;
            var cx = Katarina.botData.csecode;

            var svc =
                new CustomsearchService(new BaseClientService.Initializer
                {
                    ApiKey = apiKey
                });

            var listRequest = svc.Cse.List(query);

            listRequest.Cx = cx;

            var search = await listRequest.ExecuteAsync();

            return search;
        }


        public async Task<Url> ShortenUrlAsync(string query)
        {
            var apiKey = Katarina.botData.googleapikey;

            var shortener =
                new UrlshortenerService(new BaseClientService.Initializer
                {
                    ApiKey = apiKey
                });

            var request = await shortener.Url.Insert(new Url {LongUrl = query}).ExecuteAsync();

            return request;
        }
    }
}