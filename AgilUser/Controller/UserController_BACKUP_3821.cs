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
<<<<<<< HEAD

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
        // Test 11 - null dto
        if (request is null)
            return BadRequest(new AuthResponse { Success = false, Message = "Request body is required." });

        // test 14 - Invalid modelstate
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // test 12 - empty username 
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new AuthResponse { Success = false, Message = "Email is required." });

        // test 13 - empty password 
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthResponse { Success = false, Message = "Password is required." });

        // test 15 - exception handling 
        try
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result); // Test 10

            return Ok(result); // Test 08
        }
        catch (Exception)
        {
            return StatusCode(500, new AuthResponse { Success = false, Message = "An unexpected error occurred." });
=======
        catch
        {
            return StatusCode(500, new AuthResponse { Success = false, Message = "Internal server error." });
>>>>>>> feature/OpretProfilControllerTest
        }
    }
}