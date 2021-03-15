using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private ILoggerManager _logger;

        public WeatherForecastController(ILoggerManager logger)
        {
            this._logger = logger;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInfo("Info");
            _logger.LogDebug("Debug");
            _logger.LogWarn("Warn");
            _logger.LogError("Error");

            return new string[] { "value1", "value2" };
        }
    }
}
