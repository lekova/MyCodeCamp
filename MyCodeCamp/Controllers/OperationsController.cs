using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private IConfigurationRoot config;
        private ILogger<OperationsController> logger;

        public OperationsController(ILogger<OperationsController> logger, IConfigurationRoot config)
        {
            this.logger = logger;
            this.config = config;
        }

        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                config.Reload();

                return Ok("Configuration Reloaded");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception thrown while reloading configuration: {ex}");
            }

            return BadRequest("Could not reload configuration");
        }
    }
}
