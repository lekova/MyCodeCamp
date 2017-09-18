using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.DbUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
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
                //return Ok(mapper.Map<SpeakerModel>(speakers));
                return Ok(speakers);
            }
            catch(Exception ex)
            {

            }

            return BadRequest($"Could not retrieve speakers for {moniker}");
        }
    }
}
