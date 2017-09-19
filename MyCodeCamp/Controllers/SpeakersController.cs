using AutoMapper;
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
        public IActionResult Get(string moniker)
        {
            try
            {
                var speakers = campRepository.GetSpeakersByMoniker(moniker);
                return Ok(mapper.Map<IEnumerable<SpeakerModel>>(speakers));
            }
            catch(Exception ex)
            {
            }

            return BadRequest($"Could not retrieve speakers for {moniker}");
        }

        [HttpGet("{id}", Name = "GetSpeaker")]
        public IActionResult Get(string moniker, int id)
        {
            try
            {
                var speaker = campRepository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker)
                    return BadRequest("Speker could not be found in the specified camp.");

                return Ok(mapper.Map<SpeakerModel>(speaker));
            }
            catch (Exception ex)
            {
            }

            return BadRequest($"Could not retrieve speaker for {moniker}");
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = campRepository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest($"Could not find camp {moniker}"); 

                var speaker = mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                campRepository.Add(speaker);

                if(await campRepository.SaveAllAsync())
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
                logger.LogError($"Exception was thrown while adding a speaker for camp {moniker}.");
            }

            return BadRequest("Could not add new speaker.");
        }
    }
}
