using Microsoft.VisualBasic;
using Курсова_API.Models;
using Newtonsoft.Json;


namespace Курсова_API.Clients
{
    public class LyricsClient
    {
        private static string _address;
        private static string _apikey;
        private static string _apihost;

        public LyricsClient()
        {
            _address = Constants.Address;
            _apikey = Constants.ApiKey;
            _apihost = Constants.ApiHost;
        }
        public async Task<songLyrics.Rootobject> GetLyrics(string Song)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_address + Song),
                Headers =
        {
            { "x-rapidapi-key", _apikey },
            { "x-rapidapi-host", _apihost },
        },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);

                var Result = JsonConvert.DeserializeObject<songLyrics.Rootobject>(body);
                return Result;
            }
        }
    }
}
