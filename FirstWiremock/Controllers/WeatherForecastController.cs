using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FirstWiremock.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly bool _featureToggle;
        private readonly IHttpClientFactory _httpClientFactory;
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _featureToggle = configuration.GetValue<bool>("FeatureToggle");
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            if (_featureToggle)
            {
                var client = _httpClientFactory.CreateClient("Wiremock");
                var response = await client.GetAsync(client.BaseAddress + "/WeatherForecast");

                return await GetResponse(response);
            }
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private static async Task<IEnumerable<WeatherForecast>> GetResponse(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(result);

            return value;
        }
    }
}
