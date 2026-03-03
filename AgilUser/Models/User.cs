namespace UsersApi.Models;

// =========================
// Storage model (Repository)
// =========================
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    // Demo: gem hashed password (ikke plaintext)
    public string PasswordHash { get; set; } = string.Empty;

    // Valgfri: hvis I vil vise ekstra data senere
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

// =========================
// Request DTOs (Controller input)
// =========================
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Valgfri felter
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}}

public class LoginRequest
    // Returnér fx userId ved register/login success
    public Guid? UserId { get; set; }
{
    public string Email { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

// =========================
// Response DTO (Controller output)
