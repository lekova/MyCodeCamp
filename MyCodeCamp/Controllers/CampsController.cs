using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using System;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps")]
    public class CampsController : Controller
    {
        private ICampRepository campRepository;

        public CampsController(ICampRepository campRepository)
        {
            this.campRepository = campRepository;
        }
        public IActionResult Get()
        {
            var camps = campRepository.GetAllCamps();

            return Ok(camps);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                    camp = campRepository.GetCampWithSpeakers(id);
                else camp = campRepository.GetCamp(id);

                if (camp == null) return NotFound($"Camp {id} was not found.");
                return Ok(camp);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
