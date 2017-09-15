using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps")]
    public class CampsController : Controller
    {
        public IActionResult Get()
        {
            return Ok(new { Name = "Lina", favoriteColor = "Green" });
        }
    }
}
