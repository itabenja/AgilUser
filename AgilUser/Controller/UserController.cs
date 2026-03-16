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
                return BadRequest(result);

            return Ok(result);
        }
        catch
        {
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "Internal server error."
            });
        }
    }
}

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
        if (request is null)
            return BadRequest(new AuthResponse { Success = false, Message = "Request body is required." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new AuthResponse { Success = false, Message = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthResponse { Success = false, Message = "Password is required." });

        try
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
        catch
        {
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "An unexpected error occurred."
            });
        }
    }
}