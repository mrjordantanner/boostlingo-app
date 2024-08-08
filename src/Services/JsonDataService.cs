namespace Boostlingo.Services
{
    public class JsonDataService
    {
        private readonly HttpClient _httpClient;

        public JsonDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetJsonDataAsync(string url)
        {
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
    }

}
