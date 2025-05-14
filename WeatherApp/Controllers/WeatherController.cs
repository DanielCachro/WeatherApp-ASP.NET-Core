using Microsoft.AspNetCore.Mvc;
using WeatherApp.Services;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class WeatherController(WeatherService weatherService) : Controller
    {
        private readonly WeatherService _weatherService = weatherService;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string city)
        {
            var current = await _weatherService.GetWeatherAsync(city);
            var forecast = await _weatherService.GetForecastAsync(city);

            if (current == null || forecast == null)
            {
                ViewBag.Error = $"Nie znaleziono miasta \"{city}\".";
                return View();
            }

            var model = new WeatherAndForecast
            {
                Current = current,
                Forecast = forecast
            };

            return View(model);
        }

    }
}
