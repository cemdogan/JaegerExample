using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace Service1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBusControl _busControl;
        private readonly ITracer _tracer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBusControl busControl, ITracer tracer)
        {
            _logger = logger;
            _busControl = busControl;
            _tracer = tracer;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            WeatherForecast[] weathersForecast;
            
            using (var scope = _tracer.BuildSpan("weather-forecast").StartActive(finishSpanOnDispose: true))
            {
                var span = scope.Span.SetTag(Tags.SpanKind, Tags.SpanKindClient);

                var dictionary = new Dictionary<string, string>();
                _tracer.Inject(span.Context, BuiltinFormats.TextMap, new TextMapInjectAdapter(dictionary));

                weathersForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = Summaries[rng.Next(Summaries.Length)]
                    })
                    .ToArray();

                _busControl.Publish(new EventFromService1
                {
                    Message = "Event from Service1",
                    TracingKeys = dictionary
                });
            }

            return weathersForecast;
        }
    }
}