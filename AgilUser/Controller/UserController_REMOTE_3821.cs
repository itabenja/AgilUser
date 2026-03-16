using Microsoft.AspNetCore.Mvc;
using UsersApi.Models;
using UsersApi.Services;

namespace UsersApi.Controllers;

[ApiController]
[Route("users")]
public class UsersRegisterController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersRegisterController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validation (controller-level)
            if (request is null)
                return BadRequest(new AuthResponse { Success = false, Message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new AuthResponse { Success = false, Message = "Email is required." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new AuthResponse { Success = false, Message = "Password is required." });

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                // Duplicate email => 400 (matcher din testcase "BadRequest")
                // (jeg bruger 400 her, selvom 409 også er fint)
                return BadRequest(result);
            }

            return Ok(result); // matcher din Test 1 (200 OK)
        }
        catch
        {
            return StatusCode(500, new AuthResponse { Success = false, Message = "Internal server error." });
        }
    }
}