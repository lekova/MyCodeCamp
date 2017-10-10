using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    public class SpeakersController : BaseController
    {
        private ICampRepository campRepository;
        private ILogger logger;
        private IMapper mapper;

        public SpeakersController(ICampRepository campRepository, ILogger<SpeakersController> logger, IMapper mapper)
        {
            this.campRepository = campRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet()]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var speakers = includeTalks
                    ? campRepository.GetSpeakersByMonikerWithTalks(moniker)
                    : campRepository.GetSpeakersByMoniker(moniker);

                return Ok(mapper.Map<IEnumerable<SpeakerModel>>(speakers));
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception was thrown while retriving speakers: {ex}");
            }

            return BadRequest($"Could not retrieve speakers for {moniker}");
        }

        [HttpGet("{id}", Name = "GetSpeaker")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            try
            {
                Speaker speaker = includeTalks ? campRepository.GetSpeakerWithTalks(id) : campRepository.GetSpeaker(id);

                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker)
                    return BadRequest("Speker could not be found in the specified camp.");

                return Ok(mapper.Map<SpeakerModel>(speaker));
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception was thrown while retriving spraker: {ex}");
            }

            return BadRequest($"Could not retrieve speaker for {moniker}");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = campRepository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest($"Could not find camp {moniker}");

                var speaker = mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                campRepository.Add(speaker);

                if (await campRepository.SaveAllAsync())
                {
                    var url = Url.Link("GetSpeaker", new { moniker = camp.Moniker, id = speaker.Id });
                    return Created(url, mapper.Map<SpeakerModel>(speaker));
                }
                else
                {
                    logger.LogWarning("Could not save speaker to the database.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception was thrown while adding a speaker for camp {moniker}: {ex}");
            }

            return BadRequest("Could not add new speaker.");
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = campRepository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest($"Could not find camp {moniker}.");

                var speaker = campRepository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest();

                mapper.Map(model, speaker);

                if(await campRepository.SaveAllAsync())
                {
                    return Ok(mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Excpetion was thrown while updating speaker: {ex}");
            }
            return BadRequest("Could not update speaker.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = campRepository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest();

                campRepository.Delete(speaker);

                if (await campRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception was thrown while deleting speakers: {ex}");
            }

            return BadRequest("Could not delete speaker.");
        }
    }
}
