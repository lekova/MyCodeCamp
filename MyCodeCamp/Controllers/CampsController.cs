using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [EnableCors("AnyGet")]
    [Route("api/camps")]
    [ValidateModel]
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
                logger.LogError($"Exception was thrown while saving Camp: {ex}");
                return BadRequest();
            }
        }

        //[EnableCors("LinasPolicy")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CampModel campModel)
        {
            try
            {
                logger.LogInformation("Creating a new code camp.");

                var camp = mapper.Map<Camp>(campModel);
                campRepository.Add(camp);

                if(await campRepository.SaveAllAsync())
                {
                    var newUri = Url.Link("GetCamp", new { moniker = camp.Moniker });
                    return Created(newUri, mapper.Map<CampModel>(camp));
                }
                else
                {
                    logger.LogWarning("Could not save code camp to the database.");
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception was thrown while saving Camp: {ex}");
            }
            return BadRequest();
        }

        [EnableCors("LinasPolicy")]
        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody]CampModel campModel)
        {
            try
            {
                var oldCamp = campRepository.GetCampByMoniker(moniker);
                if(oldCamp == null)
                {
                    return NotFound($"Could not find a camp with and ID of {moniker}");
                }

                // map model to oldCamp
                mapper.Map(campModel, oldCamp);

                if (await campRepository.SaveAllAsync())
                {
                    return Ok(mapper.Map<CampModel>(oldCamp));
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception was thrown while updating Camp: {ex}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [EnableCors("LinasPolicy")]
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCapm = campRepository.GetCampByMoniker(moniker);
                if (oldCapm == null) return NotFound($"Could not find camp with ID of {moniker}");

                campRepository.Delete(oldCapm);
                if (await campRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception was thrown while deleting Camp: {ex}");
            }

            return BadRequest("Could not delete camp.");
        }
    }
}
