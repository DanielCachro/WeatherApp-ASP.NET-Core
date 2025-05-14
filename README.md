# 🌦️ WeatherApp – ASP.NET Core MVC

An application that displays the current weather and a 5-day forecast for a selected city using the OpenWeatherMap API.

## 🔧 Requirements

- [.NET 6 SDK or later](https://dotnet.microsoft.com/en-us/download)
- API key from [https://openweathermap.org/api](https://openweathermap.org/api)

## 🚀 Running Locally

1. Clone the repository:

   ```bash
   git clone https://github.com/your-repo/WeatherApp.git
   cd WeatherApp
   ```

2. Add your [OpenWeatherMap](https://openweathermap.org/api) API key in the `appsettings.Development.json` **or** `appsettings.json` file inside the `WeatherApp`:
   ```json
   {
     "OpenWeatherMap": {
       "ApiKey": "ENTER_YOUR_API_KEY_HERE"
     }
   }
   ```


3. Run the application:

   ```bash
   dotnet run --project WeatherApp
   ```

4. Open your browser at [https://localhost:5001](https://localhost:5001) or [http://localhost:5000](http://localhost:5000)