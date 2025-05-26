using Moq;
using Moq.Protected;
using WeatherApp.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;
using WeatherApp.Models;

namespace WeatherAppTests.Services
{
    [TestFixture]
    public class WeatherServiceTests
    {
        private Mock<HttpMessageHandler> _handlerMock;
        private HttpClient _httpClient;
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            // Create a mocked HttpMessageHandler to simulate HTTP requests
            _handlerMock = new Mock<HttpMessageHandler>();

            // Use the mocked handler to create a HttpClient instance
            _httpClient = new HttpClient(_handlerMock.Object);

            // Provide an in-memory configuration with a test API key
            var inMemorySettings = new Dictionary<string, string> { { "OpenWeatherMap:ApiKey", "testkey" } };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of HttpClient after each test
            _httpClient?.Dispose();
        }

        [Test]
        public async Task GetWeatherAsync_ReturnsWeatherResponse()
        {
            // Arrange: create a mock WeatherResponse and serialize it to JSON
            var response = new WeatherResponse
            {
                Name = "Warsaw",
                Main = new Main { Temp = 20.5f, Humidity = 65 },
                Weather = new List<Weather> { new Weather { Description = "Słonecznie", Icon = "01d" } }
            };
            var json = JsonSerializer.Serialize(response);

            // Mock HTTP response for a "weather" API call
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("weather")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetWeatherAsync("Warsaw");

            // Assert: verify the deserialized result matches expectations
            Assert.IsNotNull(result);
            Assert.AreEqual("Warsaw", result.Name);
            Assert.That(result.Main?.Temp, Is.EqualTo(20.5f));
            Assert.That(result.Weather?[0].Description, Is.EqualTo("Słonecznie"));
        }

        [Test]
        public async Task GetWeatherAsync_ReturnsNull_OnError()
        {
            // Arrange: mock a 404 Not Found response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("weather")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetWeatherAsync("Nowhere");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetWeatherAsync_HandlesJsonDeserializationCorrectly()
        {
            // Arrange: define a valid JSON response string
            var json = @"{
                ""name"": ""Kraków"",
                ""main"": {
                    ""temp"": 15.5,
                    ""humidity"": 72
                },
                ""weather"": [
                    {
                        ""description"": ""zachmurzenie"",
                        ""icon"": ""04d""
                    }
                ]
            }";

            // Mock the response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("weather")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetWeatherAsync("Kraków");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Kraków"));
            Assert.That(result.Main?.Temp, Is.EqualTo(15.5f));
            Assert.That(result.Main?.Humidity, Is.EqualTo(72));
            Assert.That(result.Weather?[0].Description, Is.EqualTo("zachmurzenie"));
            Assert.That(result.Weather?[0].Icon, Is.EqualTo("04d"));
        }

        [Test]
        public async Task GetWeatherAsync_ConstructsUrlCorrectly()
        {
            // Arrange: capture the outgoing HTTP request
            HttpRequestMessage capturedRequest = null;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") });

            // Act
            var service = new WeatherService(_httpClient, _config);
            await service.GetWeatherAsync("TestCity");

            // Assert: ensure the query string is built correctly
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("weather"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("q=TestCity"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("appid=testkey"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("units=metric"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("lang=pl"));
        }

        [Test]
        public async Task GetWeatherAsync_HandlesEmptyResponse()
        {
            // Arrange: return an empty JSON object
            var json = "{}";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("weather")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetWeatherAsync("SomeCity");

            // Assert: result exists but its content is null/default
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Main, Is.Null);
            Assert.That(result.Weather, Is.Null);
            Assert.That(result.Name, Is.Null);
        }

        // Forecast-related tests
        [Test]
        public async Task GetForecastAsync_ReturnsForecastResponse()
        {
            // Arrange: prepare a mock forecast response
            var forecastItem = new ForecastItem
            {
                DtTxt = DateTime.Now,
                Main = new Main { Temp = 20.5f, Humidity = 65 },
                Weather = new List<Weather> { new Weather { Description = "Słonecznie", Icon = "01d" } }
            };

            var response = new ForecastResponse
            {
                List = new List<ForecastItem> { forecastItem },
                City = new CityInfo { Name = "Warsaw" }
            };

            var json = JsonSerializer.Serialize(response);

            // Mock the forecast response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("forecast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetForecastAsync("Warsaw");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.List, Is.Not.Null);
            Assert.That(result.List.Count, Is.EqualTo(1));
            Assert.That(result.City?.Name, Is.EqualTo("Warsaw"));
            Assert.That(result.List[0].Main?.Temp, Is.EqualTo(20.5f));
        }

        [Test]
        public async Task GetForecastAsync_ReturnsNull_OnError()
        {
            // Arrange: simulate an error response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("forecast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetForecastAsync("Nowhere");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetForecastAsync_HandlesJsonDeserializationCorrectly()
        {
            // Arrange: provide valid forecast JSON
            var json = @"{
                ""list"": [
                    {
                        ""dt_txt"": ""2023-11-01 12:00:00"",
                        ""main"": {
                            ""temp"": 15.5,
                            ""humidity"": 72
                        },
                        ""weather"": [
                            {
                                ""description"": ""zachmurzenie"",
                                ""icon"": ""04d""
                            }
                        ]
                    }
                ],
                ""city"": {
                    ""name"": ""Kraków""
                }
            }";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("forecast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetForecastAsync("Kraków");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.City?.Name, Is.EqualTo("Kraków"));
            Assert.That(result.List?.Count, Is.EqualTo(1));
            Assert.That(result.List[0].Weather?[0].Description, Is.EqualTo("zachmurzenie"));
        }

        [Test]
        public async Task GetForecastAsync_ConstructsUrlCorrectly()
        {
            // Arrange: capture outgoing request for forecast
            HttpRequestMessage capturedRequest = null;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") });

            // Act
            var service = new WeatherService(_httpClient, _config);
            await service.GetForecastAsync("TestCity");

            // Assert: check the forecast URL is correctly constructed
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("forecast"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("q=TestCity"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("appid=testkey"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("units=metric"));
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain("lang=pl"));
        }

        [Test]
        public async Task GetForecastAsync_HandlesEmptyResponse()
        {
            // Arrange: return empty JSON response for forecast
            var json = "{}";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("forecast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

            // Act
            var service = new WeatherService(_httpClient, _config);
            var result = await service.GetForecastAsync("SomeCity");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.List, Is.Null);
            Assert.That(result.City, Is.Null);
        }
    }
}
