namespace UsersApi.Models;

// =========================
// Storage model (Repository)
// =========================
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    // Hashed password
    public string PasswordHash { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

// =========================
// Request DTOs (INPUT)
// =========================
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// =========================
// Response DTO (OUTPUT)
// =========================
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
}