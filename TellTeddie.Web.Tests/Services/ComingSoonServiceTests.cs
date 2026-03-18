using Microsoft.Extensions.Configuration;
using Moq;
using TellTeddie.Web.Services;

namespace TellTeddie.Web.Tests.Services
{
    public class ComingSoonServiceTests
    {
        [Fact]
        public void IsComingSoonMode_ReturnsFalse_WhenApiBaseAddressIsConfigured()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns("https://api.tellteddie.com");

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsComingSoonMode_ReturnsTrue_WhenApiBaseAddressIsNull()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns((string?)null);

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsComingSoonMode_ReturnsTrue_WhenApiBaseAddressIsEmpty()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns(string.Empty);

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsComingSoonMode_ReturnsTrue_WhenApiBaseAddressIsComingSoon()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns("coming-soon");

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsComingSoonMode_ReturnsFalse_WhenApiBaseAddressIsLocalhost()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns("http://localhost:5029");

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsComingSoonMode_ReturnsFalse_WhenApiBaseAddressIsAzureUrl()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns("https://tellteddie-api.azurewebsites.net");

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("coming-soon")]
        public void IsComingSoonMode_ReturnsTrue_ForAllComingSoonScenarios(string? apiAddress)
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns(apiAddress);

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("https://api.tellteddie.com")]
        [InlineData("http://localhost:5029")]
        [InlineData("https://tellteddie-api.azurewebsites.net")]
        [InlineData("http://192.168.1.1:5000")]
        public void IsComingSoonMode_ReturnsFalse_ForAllProductionScenarios(string apiAddress)
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(c => c["ApiBaseAddress"])
                .Returns(apiAddress);

            var service = new ComingSoonService(configurationMock.Object);

            // Act
            var result = service.IsComingSoonMode();

            // Assert
            Assert.False(result);
        }
    }
}
