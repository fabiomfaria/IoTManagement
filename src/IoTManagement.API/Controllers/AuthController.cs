using System;
using System.Threading.Tasks;
using IoTManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IoTManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserStore _userStore;
        private readonly ITokenService _tokenService;

        public AuthController(IUserStore userStore, ITokenService tokenService)
        {
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpPost("token")]
        public ActionResult GetToken([FromForm] TokenRequest request)
        {
            // Validate grant type
            if (request.GrantType != "password")
            {
                return BadRequest(new { error = "unsupported_grant_type" });
            }

            // Validate user credentials
            var user = _userStore.ValidateUser(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { error = "invalid_grant", error_description = "Invalid username or password" });
            }

            // Generate access token
            var accessToken = _tokenService.GenerateAccessToken(user);

            // Return OAuth2 token response
            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour in seconds
                Scope = "api"
            });
        }
    }

    public class TokenRequest
    {
        public string GrantType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }
}