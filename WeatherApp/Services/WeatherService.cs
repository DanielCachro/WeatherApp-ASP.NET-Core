namespace WeatherApp.Services
{
    using Newtonsoft.Json;
    using WeatherApp.Models;

    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? throw new ArgumentNullException("ApiKey", "API key is required.");
        }

        public async Task<WeatherResponse?> GetWeatherAsync(string city)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric&lang=pl";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(json);
        }

        public async Task<ForecastResponse?> GetForecastAsync(string city)
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric&lang=pl";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ForecastResponse>(json);
        }
    }

}
