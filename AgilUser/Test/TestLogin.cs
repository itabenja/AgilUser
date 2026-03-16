using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UsersApi.Controllers;
using UsersApi.Models;
using UsersApi.Repositories;
using UsersApi.Services;

namespace AgilUser.Test;

[TestClass]
public class TestLogin
{
    private Mock<IUserRepository> _mockRepo;
    private Mock<IAuthService> _mockAuthService;
    private UsersLoginController _controller;
    private User _seededUser;
    private const string SeededPassword = "correctPassword";
    private const string SeededEmail = "test@test.com";

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockAuthService = new Mock<IAuthService>();
        _controller = new UsersLoginController(_mockAuthService.Object);

        //  Seed data
        var hasher = new PasswordHasher<User>();
        _seededUser = new User
        {
            Id = Guid.NewGuid(),
            Email = SeededEmail,
            FirstName = "Test",
            LastName = "User",
            CreatedAtUtc = DateTime.UtcNow
        };
        _seededUser.PasswordHash = hasher.HashPassword(_seededUser, SeededPassword);
    }

    // Test 08 — Valid credentials → 200 OK
    [TestMethod]
    public async Task Test08_ValidCredentials_ShouldReturn200()
    {
        // Arrange
        _mockAuthService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new AuthResponse { Success = true, Message = "Login successful.", UserId = _seededUser.Id });

        var request = new LoginRequest { Email = SeededEmail, Password = SeededPassword };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    // Test 09 — Repository called with correct email
    [TestMethod]
    public async Task Test09_Repository_ShouldBeCalledWithCorrectEmail()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetByEmailAsync(SeededEmail))
            .ReturnsAsync(_seededUser);

        var authService = new AuthService(_mockRepo.Object);
        var request = new LoginRequest { Email = SeededEmail, Password = SeededPassword };

        // Act
        await authService.LoginAsync(request);

        // Assert — verificer repository blev kaldt med præcis den rigtige email
        _mockRepo.Verify(r => r.GetByEmailAsync(SeededEmail), Times.Once);
    }

    // Test 10 — Wrong password / user not found → 401
    [TestMethod]
    public async Task Test10_WrongPassword_ShouldReturn401()
    {
        // Arrange
        _mockAuthService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new AuthResponse { Success = false, Message = "Invalid email or password." });

        var request = new LoginRequest { Email = "ingen@test.com", Password = "forkert" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    // Test 11 — Null DTO → 400
    [TestMethod]
    public async Task Test11_NullRequest_ShouldReturn400()
    {
        // Act
        var result = await _controller.Login(null);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    // Test 12 — Empty email → 400
    [TestMethod]
    public async Task Test12_EmptyEmail_ShouldReturn400()
    {
        // Arrange
        var request = new LoginRequest { Email = "", Password = SeededPassword };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    // Test 13 — Empty password → 400
    [TestMethod]
    public async Task Test13_EmptyPassword_ShouldReturn400()
    {
        // Arrange
        var request = new LoginRequest { Email = SeededEmail, Password = "" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    //  Test 14 — Invalid ModelState → 400
    [TestMethod]
    public async Task Test14_InvalidModelState_ShouldReturn400()
    {
        // Arrange — simuler invalid modelstate
        _controller.ModelState.AddModelError("Email", "Email is not valid");
        var request = new LoginRequest { Email = "ikke-en-email", Password = SeededPassword };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    // Test 15 — Repository throws exception → 500
    [TestMethod]
    public async Task Test15_RepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockAuthService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new Exception("Database exploded!"));

        var request = new LoginRequest { Email = SeededEmail, Password = SeededPassword };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var serverError = result as ObjectResult;
        Assert.IsNotNull(serverError);
        Assert.AreEqual(500, serverError.StatusCode);
    }
}