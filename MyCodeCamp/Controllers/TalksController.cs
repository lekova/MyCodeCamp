using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using MyCodeCamp.DbUtilities;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers/{speakerId}/talks")]
    [ValidateModel]
    public class TalksController : BaseController
    {
        private ILogger<TalksController> logger;
        private IMapper mapper;
        private ICampRepository repo;

        public TalksController(ICampRepository repo, ILogger<TalksController> logger, IMapper mapper)
        {
            this.repo = repo;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(string moniker, int speakerId)
        {
            var talks = repo.GetTalks(speakerId);

            if (talks.Any(t => t.Speaker.Camp.Moniker != moniker))
                return BadRequest("Invalid talks for the speaker selected");

            return Ok(mapper.Map<IEnumerable<TalkModel>>(talks));
        }

        [HttpGet("{id}", Name = "GetTalk")]
        public IActionResult Get(string moniker, int speakerId, int id)
        {
            var talk = repo.GetTalk(id);

            if (talk.Speaker.Id != speakerId || talk.Speaker.Camp.Moniker != moniker) return BadRequest("Invalid talk for the speaker selected");

            return Ok(mapper.Map<TalkModel>(talk));
        }

        [HttpPost()]
        public async Task<IActionResult> Post(string moniker, int speakerId, [FromBody] TalkModel model)
        {
            try
            {
                var speaker = repo.GetSpeaker(speakerId);
                if (speaker != null)
                {
                    var talk = mapper.Map<Talk>(model);

                    talk.Speaker = speaker;
                    repo.Add(talk);

                    if (await repo.SaveAllAsync())
                    {
                        return Created(Url.Link("GetTalk", new { moniker = moniker, speakerId = speakerId, id = talk.Id }), mapper.Map<TalkModel>(talk));
                    }
                }

            }
            catch (Exception ex)
            {

                logger.LogError($"Failed to save new talk: {ex}");
            }

            return BadRequest("Failed to save new talk");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int speakerId, int id, [FromBody] TalkModel model)
        {
            try
            {
                var talk = repo.GetTalk(id);
                if (talk == null) return NotFound();

                mapper.Map(model, talk);

                if (await repo.SaveAllAsync())
                {
                    return Ok(mapper.Map<TalkModel>(talk));
                }

            }
            catch (Exception ex)
            {

                logger.LogError($"Failed to update talk: {ex}");
            }

            return BadRequest("Failed to update talk");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int speakerId, int id)
        {
            try
            {
                var talk = repo.GetTalk(id);
                if (talk == null) return NotFound();

                repo.Delete(talk);

                if (await repo.SaveAllAsync())
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {

                logger.LogError($"Failed to delete talk: {ex}");
            }

            return BadRequest("Failed to delete talk");
        }

    }
}
