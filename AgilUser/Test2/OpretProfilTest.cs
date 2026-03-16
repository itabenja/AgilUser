using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UsersApi.Controllers;
using UsersApi.Models;
using UsersApi.Services;

namespace AgilUser.Test;

[TestClass]
public class TestRegister
{
    private Mock<IAuthService> _mockAuthService;
    private UsersRegisterController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new UsersRegisterController(_mockAuthService.Object);
    }

    // Test 1 — Valid Registration: Returns 200 OK and user is added
    [TestMethod]
    public async Task Test01_ValidRegistration_ShouldReturn200()
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

        _mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var body = okResult.Value as AuthResponse;
        Assert.IsNotNull(body);
        Assert.IsTrue(body.Success);

        _mockAuthService.Verify(s =>
            s.RegisterAsync(It.Is<RegisterRequest>(r => r.Email == request.Email)),
            Times.Once);
    }

    // Test 2 — Email already exists: Returns 400 BadRequest
    [TestMethod]
    public async Task Test02_EmailAlreadyExists_ShouldReturn400()
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

        _mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);

        var body = badRequest.Value as AuthResponse;
        Assert.IsNotNull(body);
        Assert.IsFalse(body.Success);
    }

    // Test 3 — Null DTO: Returns 400 BadRequest
    [TestMethod]
    public async Task Test03_NullRequest_ShouldReturn400()
    {
        // Act
        var result = await _controller.Register(null);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);

        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 4 — Empty email: Returns 400 BadRequest
    [TestMethod]
    public async Task Test04_EmptyEmail_ShouldReturn400()
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
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);

        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 5 — Empty password: Returns 400 BadRequest
    [TestMethod]
    public async Task Test05_EmptyPassword_ShouldReturn400()
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
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);

        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 6 — Whitespace email: Returns 400 BadRequest
    [TestMethod]
    public async Task Test06_WhitespaceEmail_ShouldReturn400()
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
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);

        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // Test 7 — Service throws exception: Returns 500 InternalServerError
    [TestMethod]
    public async Task Test07_ServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Secret123!"
        };

        _mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new Exception("DB broke"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var serverError = result as ObjectResult;
        Assert.IsNotNull(serverError);
        Assert.AreEqual(500, serverError.StatusCode);
    }
}