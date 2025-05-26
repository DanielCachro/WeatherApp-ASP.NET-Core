using HtmlAgilityPack;

namespace WeatherAppTests.Views
{
    [TestFixture]
    public class WeatherViewHtmlTests
    {
        private HtmlDocument _document;
        private string _htmlContent;

        [SetUp]
        public void Setup()
        {
            // Load the HTML content from the view file
            string filePath = Path.Combine("..", "..", "..", "..", "WeatherApp", "Views", "Weather", "Index.cshtml");
            _htmlContent = File.ReadAllText(filePath);

            _document = new HtmlDocument();
            _document.LoadHtml(_htmlContent);
        }

        [Test]
        public void WeatherView_HasCorrectTitle()
        {
            // Check if the view has the correct title
            Assert.That(_htmlContent.Contains("ViewData[\"Title\"] = \"Pogoda\""), Is.True, "Page title should be 'Pogoda'");
        }

        [Test]
        public void WeatherView_ContainsSearchForm()
        {
            // Check if the form exists with correct attributes
            var form = _document.DocumentNode.SelectSingleNode("//form");
            Assert.That(form, Is.Not.Null, "Search form not found");
            Assert.That(form.GetAttributeValue("method", ""), Is.EqualTo("post"), "Form should use POST method");
            Assert.That(form.OuterHtml.Contains("asp-action=\"Index\""), Is.True, "Form should target Index action");

            // Check for city input field
            var inputElement = _document.DocumentNode.SelectSingleNode("//input[@name='city']");
            Assert.That(inputElement, Is.Not.Null, "City input field not found");
            Assert.That(inputElement.GetAttributeValue("type", ""), Is.EqualTo("text"), "Input field should be of type text");
            Assert.That(inputElement.GetAttributeValue("placeholder", ""), Is.EqualTo("Wpisz miasto"), "Placeholder should be 'Wpisz miasto'");
            Assert.That(inputElement.HasAttributes && inputElement.Attributes.Contains("required"), Is.True, "Input field should be required");

            // Check for search button
            var button = _document.DocumentNode.SelectSingleNode("//button[@type='submit']");
            Assert.That(button, Is.Not.Null, "Submit button not found");
            Assert.That(button.InnerText.Contains("Szukaj"), Is.True, "Button should contain 'Szukaj' text");
        }

        [Test]
        public void WeatherView_ContainsErrorMessageSection()
        {
            // Check for error message display section
            var errorConditional = _htmlContent.Contains("@if (ViewBag.Error != null)");
            Assert.That(errorConditional, Is.True, "Missing condition for displaying error message");

            var errorDiv = _document.DocumentNode.SelectNodes("//div")
                .FirstOrDefault(d => d.HasClass("alert") && d.HasClass("alert-danger") && d.HasClass("text-center"));

            Assert.That(errorDiv, Is.Not.Null, "Error message div not found");
            Assert.That(_htmlContent.Contains("@ViewBag.Error"), Is.True, "Missing ViewBag.Error display");
        }

        [Test]
        public void WeatherView_ContainsCurrentWeatherSection()
        {
            // Check for current weather section
            var currentWeatherConditional = _htmlContent.Contains("@if (Model?.Current?.Main != null && Model.Current.Weather?.Any() == true)");
            Assert.That(currentWeatherConditional, Is.True, "Missing condition for displaying current weather");

            // Check the structure of the current weather card
            var cardDiv = _document.DocumentNode.SelectNodes("//div")
                .FirstOrDefault(d => d.HasClass("card") && d.HasClass("mb-4") && d.HasClass("shadow-sm"));
            Assert.That(cardDiv, Is.Not.Null, "Current weather card not found");

            var cardBody = _htmlContent.Contains("<div class=\"card-body\">");
            Assert.That(cardBody, Is.True, "Missing card-body section for current weather");

            var cardTitle = _htmlContent.Contains("<h4 class=\"card-title\">Aktualna pogoda w @Model.Current.Name</h4>");
            Assert.That(cardTitle, Is.True, "Missing card title for current weather");

            // Check weather information fields
            var temperatureField = _htmlContent.Contains("<strong>Temperatura:</strong> @Model.Current.Main.Temp.ToString(\"0\") °C");
            Assert.That(temperatureField, Is.True, "Missing temperature field");

            var humidityField = _htmlContent.Contains("<strong>Wilgotność:</strong> @Model.Current.Main.Humidity%");
            Assert.That(humidityField, Is.True, "Missing humidity field");

            var descriptionField = _htmlContent.Contains("<strong>Opis:</strong> @Model.Current.Weather[0].Description");
            Assert.That(descriptionField, Is.True, "Missing weather description field");

            // Check for weather icon
            var weatherIconImg = _htmlContent.Contains("<img src=\"http://openweathermap.org/img/wn/@($\"{Model.Current.Weather[0].Icon}@2x.png\")\" alt=\"Ikona pogody\" />");
            Assert.That(weatherIconImg, Is.True, "Missing weather icon image");
        }

        [Test]
        public void WeatherView_ContainsForecastSection()
        {
            // Check for 5-day forecast section
            var forecastConditional = _htmlContent.Contains("@if (Model?.Forecast?.List != null)");
            Assert.That(forecastConditional, Is.True, "Missing condition for displaying forecast");

            var forecastHeader = _htmlContent.Contains("<h3 class=\"mb-3\">Prognoza 5-dniowa (co 3 godziny)</h3>");
            Assert.That(forecastHeader, Is.True, "Missing forecast section header");

            // Check if the view groups data by days
            var groupByDay = _htmlContent.Contains(".GroupBy(f => f.DtTxt.Date)");
            Assert.That(groupByDay, Is.True, "Missing forecast grouping by day");

            var orderByDate = _htmlContent.Contains(".OrderBy(g => g.Key)");
            Assert.That(orderByDate, Is.True, "Missing forecast sorting by date");

            // Check date formatting
            var dateFormatting = _htmlContent.Contains("@dayGroup.Key.ToString(\"dddd, dd.MM.yyyy\", new System.Globalization.CultureInfo(\"pl-PL\"))");
            Assert.That(dateFormatting, Is.True, "Missing proper date formatting with Polish culture");

            // Check the structure of individual forecast cards
            var forecastCards = _htmlContent.Contains("<div class=\"card h-100 shadow-sm\">");
            Assert.That(forecastCards, Is.True, "Missing cards for forecast items");

            var timeDisplay = _htmlContent.Contains("<h6 class=\"card-subtitle mb-2 text-muted\">@item.DtTxt.ToString(\"HH:mm\")</h6>");
            Assert.That(timeDisplay, Is.True, "Missing time display in forecast");

            var tempDisplay = _htmlContent.Contains("<strong>@item.Main?.Temp.ToString(\"0\")</strong> °C");
            Assert.That(tempDisplay, Is.True, "Missing temperature display in forecast");

            var descDisplay = _htmlContent.Contains("@item.Weather?[0]?.Description");
            Assert.That(descDisplay, Is.True, "Missing weather description in forecast");

            // Check conditional icon display
            var iconConditional = _htmlContent.Contains("@if (item.Weather?[0]?.Icon != null)");
            Assert.That(iconConditional, Is.True, "Missing condition for weather icon display");

            var iconDisplay = _htmlContent.Contains("<img src=\"http://openweathermap.org/img/wn/@($\"{item.Weather[0].Icon}@2x.png\")\" alt=\"Ikona pogody\" />");
            Assert.That(iconDisplay, Is.True, "Missing weather icon image in forecast");
        }

        [Test]
        public void WeatherView_HasResponsiveDesign()
        {
            // Check if the view uses Bootstrap responsive classes
            var containerClass = _htmlContent.Contains("<div class=\"container mt-5\">");
            Assert.That(containerClass, Is.True, "Missing main container with appropriate spacing");

            // Check form with responsive classes
            var responsiveForm = _htmlContent.Contains("class=\"row g-3 justify-content-center mb-4\"");
            Assert.That(responsiveForm, Is.True, "Form doesn't use Bootstrap responsive classes");

            var colAuto = _htmlContent.Contains("<div class=\"col-auto\">");
            Assert.That(colAuto, Is.True, "Missing col-auto classes for automatic width columns");

            // Check forecast layout
            var forecastGrid = _htmlContent.Contains("<div class=\"row row-cols-1 row-cols-md-3 g-3\">");
            Assert.That(forecastGrid, Is.True, "Missing responsive grid for forecast (1 column on small screens, 3 on medium)");

            var colClass = _htmlContent.Contains("<div class=\"col\">");
            Assert.That(colClass, Is.True, "Missing standard columns for forecast items");
        }

        [Test]
        public void WeatherView_UsesBootstrapComponents()
        {
            // Check for Bootstrap components usage
            var buttonPrimary = _htmlContent.Contains("class=\"btn btn-primary\"");
            Assert.That(buttonPrimary, Is.True, "Missing button with btn-primary class");

            var alertDanger = _htmlContent.Contains("class=\"alert alert-danger text-center\"");
            Assert.That(alertDanger, Is.True, "Missing alert with alert-danger class for errors");

            var cardComponents = _htmlContent.Contains("card-body") && _htmlContent.Contains("card-title") &&
                _htmlContent.Contains("card-text") && _htmlContent.Contains("card-subtitle");
            Assert.That(cardComponents, Is.True, "Missing standard classes for card components");
        }
    }
}
