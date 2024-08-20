/*using LudoLab_ConnectSys_Server.Services;
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

}*/

/*
using LudoLab_ConnectSys_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            try
            {
                var token = await _tokenService.GetAccessTokenAsync();
                return Ok(token);
            }
            catch (Exception ex)
            {
                // Maneja cualquier error durante la obtención del token
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
*/

using LudoLab_ConnectSys_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [ApiController]
    [Route("api/graphauth")]
    public class GraphAuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public GraphAuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync();
                return Ok(token);
            }
            catch (Exception ex)
            {
                // Maneja cualquier error durante la obtención del token
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
