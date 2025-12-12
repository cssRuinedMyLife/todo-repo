using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Models;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public class GoogleLoginRequest
        {
            public string Token { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        [HttpPost("google")]
        public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await _authService.ValidateGoogleTokenAsync(request.Token);
            }
            catch (Exception)
            {
                return BadRequest("Invalid Google Token");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    GoogleSubjectId = payload.Subject
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Optional: Update user info if changed
                if (user.GoogleSubjectId == null)
                {
                    user.GoogleSubjectId = payload.Subject;
                    await _context.SaveChangesAsync();
                }
            }

            var jwt = _authService.GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                Token = jwt,
                Email = user.Email,
                Name = user.Name
            });
        }
    }
}
