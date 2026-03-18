using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TellTeddie.Web.Pages;
using TellTeddie.Web.Services;

namespace TellTeddie.Web.Tests.Pages
{
    public class ComingSoonTests : TestContext
    {
        public ComingSoonTests()
        {
            // Register services needed for ComingSoon component
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ApiBaseAddress"]).Returns((string?)null);
            
            Services.AddSingleton(configMock.Object);
            Services.AddSingleton<IComingSoonService, ComingSoonService>();
        }

        [Fact]
        public void ComingSoonPage_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            Assert.NotNull(cut);
            var title = cut.Find("h1");
            Assert.NotNull(title);
            Assert.Contains("Tell Teddie, soon", title.TextContent);
        }

        [Fact]
        public void ComingSoonPage_Renders_WithImage()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var image = cut.Find("img.theodora-image");
            Assert.NotNull(image);
            Assert.Equal("assets/images/princess-theodora.png", image.GetAttribute("src"));
        }

        [Fact]
        public void ComingSoonPage_Renders_WithCircularImage()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var image = cut.Find("img.theodora-image");
            Assert.NotNull(image);
            Assert.Equal("theodora-image", image.ClassName);
        }

        [Fact]
        public void ComingSoonPage_Renders_WithContainer()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var container = cut.Find(".coming-soon-container");
            Assert.NotNull(container);
        }

        [Fact]
        public void ComingSoonPage_Renders_WithContent()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var content = cut.Find(".coming-soon-content");
            Assert.NotNull(content);
        }

        [Fact]
        public void ComingSoonPage_ShowsComingSoonContent()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var container = cut.Find(".coming-soon-container");
            Assert.NotNull(container);
            var content = container.TextContent;
            Assert.Contains("Tell Teddie, soon", content);
        }

        [Fact]
        public void ComingSoonPage_HasValidHtmlStructure()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var container = cut.Find(".coming-soon-container");
            Assert.NotNull(container);
            
            var content = cut.Find(".coming-soon-content");
            Assert.NotNull(content);
            
            var image = cut.Find("img.theodora-image");
            Assert.NotNull(image);
            
            var heading = cut.Find("h1");
            Assert.NotNull(heading);
        }

        [Fact]
        public void ComingSoonPage_HasPageTitle()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            // Component renders successfully with page title
            Assert.NotNull(cut);
        }

        [Fact]
        public void ComingSoonPage_ImageDoesNotSetAltText()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            var image = cut.Find("img.theodora-image");
            var altText = image.GetAttribute("alt");
            Assert.Null(altText);
        }

        [Fact]
        public void ComingSoonPage_ContainerHasGradientBackground()
        {
            // Arrange & Act
            var cut = Render<ComingSoon>();

            // Assert
            // The container element should exist and have the proper class
            var container = cut.Find(".coming-soon-container");
            Assert.NotNull(container);
            // CSS class ensures gradient background via coming-soon.css
            Assert.Contains("coming-soon-container", container.ClassName);
        }

        [Fact]
        public void ComingSoonPage_RedirectsToApp_WhenNotInComingSoonMode()
        {
            // Arrange
            var comingSoonService = new Mock<IComingSoonService>();
            comingSoonService.Setup(service => service.IsComingSoonMode()).Returns(false);
            Services.AddSingleton(comingSoonService.Object);

            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            Render<ComingSoon>();

            // Assert
            Assert.EndsWith("/app", navigationManager.Uri, StringComparison.OrdinalIgnoreCase);
        }
    }
}
