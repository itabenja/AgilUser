using Microsoft.AspNetCore.Identity;
using UsersApi.Models;
using UsersApi.Repositories;

namespace UsersApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Service-level validation (robust)
        if (request is null)
            return new AuthResponse { Success = false, Message = "Request is required." };

        var email = request.Email?.Trim();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email))
            return new AuthResponse { Success = false, Message = "Email is required." };

        if (string.IsNullOrWhiteSpace(password))
            return new AuthResponse { Success = false, Message = "Password is required." };

        // Email unique
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing is not null)
            return new AuthResponse { Success = false, Message = "Email already exists." };

        // Create user + hash password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _userRepository.AddAsync(user);

        return new AuthResponse
        {
            Success = true,
            Message = "User registered successfully.",
            UserId = user.Id
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (request is null)
            return new AuthResponse { Success = false, Message = "Request is required." };

        var email = request.Email?.Trim();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email))
            return new AuthResponse { Success = false, Message = "Email is required." };

        if (string.IsNullOrWhiteSpace(password))
            return new AuthResponse { Success = false, Message = "Password is required." };

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            return new AuthResponse { Success = false, Message = "Invalid email or password." };

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verify == PasswordVerificationResult.Failed)
            return new AuthResponse { Success = false, Message = "Invalid email or password." };

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.Id
        };
    }
    
    public async Task<List<User>> GetUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }
}