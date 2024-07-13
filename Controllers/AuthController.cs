using LudoLab_ConnectSys_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            return Ok(token);
        }
    }

}
