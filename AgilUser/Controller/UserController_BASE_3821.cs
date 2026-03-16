using Microsoft.AspNetCore.Mvc;
using UsersApi.Models;
using UsersApi.Services; // hvor IAuthService ligger

namespace UsersApi.Controllers;

// =========================
// 1) Register Controller
// POST /users/register
// =========================
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
        // Basic validation (controller-level)
        if (request is null)
            return BadRequest(new AuthResponse { Success = false, Message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new AuthResponse { Success = false, Message = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthResponse { Success = false, Message = "Password is required." });

        // Service call (unique email + hashing sker i service)
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            // typisk: email allerede brugt eller input invalid
            // 409 Conflict giver god mening for duplicate email
            if (result.Message.Contains("already", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("unique", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }

        // 201 Created (vi har oprettet en ny user)
        // Vi har også GET /users/{id} i sprint 2, så vi kan returnere Location
        return Created($"/users/{result.UserId}", result);
    }
}

// =========================
// 2) Login Controller
// POST /users/login
// =========================
[ApiController]
[Route("users")]
public class UsersLoginController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersLoginController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Basic validation
        if (request is null)
            return BadRequest(new AuthResponse { Success = false, Message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new AuthResponse { Success = false, Message = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthResponse { Success = false, Message = "Password is required." });

        // Service call (verify user + verify hashed password sker i service)
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            // 401 Unauthorized ved forkert login
            return Unauthorized(result);
        }

        return Ok(result);
    }
}