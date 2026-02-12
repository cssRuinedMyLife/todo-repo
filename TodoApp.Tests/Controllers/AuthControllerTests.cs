using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoApp.Api.Controllers;
using TodoApp.Api.Data;
using TodoApp.Api.Models;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AppDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _controller = new AuthController(_mockAuthService.Object, _context);
        }

        [Fact]
        public async Task GoogleLogin_ValidToken_CreatesUser_And_ReturnsToken()
        {
            // Arrange
            var googleToken = "valid-token";
            var googlePayload = new GoogleJsonWebSignature.Payload
            {
                Email = "test@example.com",
                Name = "Test User",
                Subject = "google-sub-id"
            };
            var generatedJwt = "our-jwt-token";

            _mockAuthService.Setup(s => s.ValidateGoogleTokenAsync(googleToken))
                .ReturnsAsync(googlePayload);
            _mockAuthService.Setup(s => s.GenerateJwtToken(It.IsAny<User>()))
                .Returns(generatedJwt);

            var request = new AuthController.GoogleLoginRequest { Token = googleToken };

            // Act
            var result = await _controller.GoogleLogin(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthController.LoginResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AuthController.LoginResponse>(okResult.Value);

            Assert.Equal(generatedJwt, response.Token);
            Assert.Equal(googlePayload.Email, response.Email);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == googlePayload.Email);
            Assert.NotNull(userInDb);
            Assert.Equal(googlePayload.Name, userInDb.Name);
            Assert.Equal(googlePayload.Subject, userInDb.GoogleSubjectId);
        }

        [Fact]
        public async Task GoogleLogin_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var googleToken = "invalid-token";
            _mockAuthService.Setup(s => s.ValidateGoogleTokenAsync(googleToken))
                .ThrowsAsync(new Exception("Invalid token"));

            var request = new AuthController.GoogleLoginRequest { Token = googleToken };

            // Act
            var result = await _controller.GoogleLogin(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthController.LoginResponse>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
    }
}
