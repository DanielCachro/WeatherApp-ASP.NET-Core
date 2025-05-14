using Newtonsoft.Json;

namespace WeatherApp.Models
{
    public class ForecastResponse
    {
        public List<ForecastItem>? List { get; set; }
        public CityInfo? City { get; set; }
    }

    public class ForecastItem
    {
        [JsonProperty("dt_txt")]
        public DateTime DtTxt { get; set; } 
        public Main? Main { get; set; }
        public List<Weather>? Weather { get; set; }
    }

    public class CityInfo
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
    }

}
