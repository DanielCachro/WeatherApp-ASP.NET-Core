namespace WeatherApp.Models
{
    public class WeatherAndForecast
    {
        public WeatherResponse? Current { get; set; }
        public ForecastResponse? Forecast { get; set; }
    }
}
