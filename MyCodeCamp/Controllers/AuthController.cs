using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    public class AuthController : Controller
    {
        private CampContext context;
        private SignInManager<CampUser> signInMgr;
        private ILogger<AuthController> logger;

        public AuthController(CampContext context, SignInManager<CampUser> signInMgr, ILogger<AuthController> logger)
        {
            this.context = context;
            this.signInMgr = signInMgr;
            this.logger = logger;
        }

        [EnableCors("LoginPost")]
        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                // isPersisted = false => it is not stored in the browser after the browser is closed
                // lockoutOnFailure => not lockout the user after this unsuccessful attempt to login
                var result = await signInMgr.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception was thrown wile authenticating - {ex}");
            }
            return BadRequest("Failed to login.");
        }
    }
}
