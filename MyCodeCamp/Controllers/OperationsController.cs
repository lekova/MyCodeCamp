using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/operations")]
    public class OperationsController : Controller
    {
        private ILogger<OperationsController> logger;
        private IConfigurationRoot config;

        public OperationsController(ILogger<OperationsController> logger, IConfigurationRoot configurationRoot)
        {
            this.logger = logger;
            this.config = configurationRoot;
        }

        [HttpGet("reloadConfig")]
        public IActionResult ReploadConfiguration()
        {
            try
            {
                config.Reload();
                return Ok("Configuration reloaded");
            }
            catch (Exception ex)
            {
                logger.LogError($"Excdeption was thrown while reloading configuration: {ex}");
            }
            return BadRequest("Could not reload configuration.");
        }
    }
}
