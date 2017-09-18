using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps")]
    public class CampsController : BaseController
    {
        private ICampRepository campRepository;
        private ILogger<CampsController> logger;
        private IMapper mapper;

        public CampsController(ICampRepository campRepository, ILogger<CampsController> logger, 
            IMapper mapper)
        {
            this.campRepository = campRepository;
            this.logger = logger;
            this.mapper = mapper;
        }
        public IActionResult Get()
        {
            var camps = campRepository.GetAllCamps();

            return Ok(mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name ="GetCamp")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                    camp = campRepository.GetCampByMonikerWithSpeakers(moniker);
                else camp = campRepository.GetCampByMoniker(moniker);

                if (camp == null) return NotFound($"Camp {moniker} was not found.");
                return Ok(mapper.Map<CampModel>(camp));
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Camp campModel)
        {
            logger.LogInformation("Creating a new code camp.");
            try
            {
                campRepository.Add(campModel);
                if(await campRepository.SaveAllAsync())
                {
                    var newUri = Url.Link("GetCamp", new { id = campModel.Id });
                    return Created(newUri, campModel);
                }
                else
                {
                    logger.LogWarning("Could not save code camp to the database.");
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Threw exception while saving Camp: {ex}");
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Camp campModel)
        {
            try
            {
                var oldCamp = campRepository.GetCamp(id);
                if(oldCamp == null)
                {
                    return NotFound($"Could not find a camp with and ID of {id}");
                }

                // map model to oldCamp
                oldCamp.Name = campModel.Name ?? oldCamp.Name;
                oldCamp.Description = campModel.Description ?? oldCamp.Description;
                oldCamp.Location = campModel.Location ?? oldCamp.Location;
                oldCamp.Length = campModel.Length > 0 ? campModel.Length : oldCamp.Length;
                oldCamp.EventDate = 
                    campModel.EventDate != DateTime.MinValue ? campModel.EventDate : oldCamp.EventDate;

                if (await campRepository.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
            }
            catch(Exception ex)
            {

            }

            return BadRequest("Couldn't update Camp");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldCapm = campRepository.GetCamp(id);
                if (oldCapm == null) return NotFound($"Could not find camp with ID of {id}");

                campRepository.Delete(oldCapm);
                if (await campRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch(Exception ex)
            {

            }

            return BadRequest("Could not delete camp.");
        }
    }
}
