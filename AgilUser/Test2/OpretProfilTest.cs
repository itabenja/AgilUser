using Microsoft.AspNetCore.Mvc;
using Moq;
using UsersApi.Controllers;
using UsersApi.Models;
using UsersApi.Services;
using Xunit;

namespace UsersApi.Tests.Controllers;

public class UsersRegisterControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly UsersRegisterController _controller;

    public UsersRegisterControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>(MockBehavior.Strict);
        _controller = new UsersRegisterController(_authServiceMock.Object);
    }

    // Test 1 — Valid Registration: Returns 200 OK and user is added
    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Secret123!"
        };

        var response = new AuthResponse
        {
            Success = true,
            Message = "User registered successfully.",
            UserId = Guid.NewGuid()
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);

        var body = Assert.IsType<AuthResponse>(ok.Value);
        Assert.True(body.Success);

        _authServiceMock.Verify(s =>
            s.RegisterAsync(It.Is<RegisterRequest>(r => r.Email == request.Email)),
            Times.Once);
    }

    // Test 2 — Username Already Exists: Returns 400 BadRequest
    [Fact]
    public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "taken@example.com",
            Password = "Secret123!"
        };

        var response = new AuthResponse
        {
            Success = false,
            Message = "Email already exists."
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var body = Assert.IsType<AuthResponse>(bad.Value);
        Assert.False(body.Success);
    }

    // Test 3 — Null DTO: Returns 400 BadRequest
    [Fact]
    public async Task Register_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Register(null!);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        // Service should NOT be called
        _authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 4 — Empty Username (Email): Returns 400 BadRequest
    [Fact]
    public async Task Register_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "",
            Password = "Secret123!"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        _authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 5 — Empty Password: Returns 400 BadRequest
    [Fact]
    public async Task Register_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        _authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 6 — Whitespace Username (Email): Returns 400 BadRequest
    [Fact]
    public async Task Register_WhitespaceEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "   ",
            Password = "Secret123!"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        _authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 7 — Repository Throws Exception During Add: Returns 500 InternalServerError
    [Fact]
    public async Task Register_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Secret123!"
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new Exception("DB broke"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}