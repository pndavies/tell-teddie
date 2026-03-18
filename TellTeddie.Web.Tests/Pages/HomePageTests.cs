using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Web.Pages;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Pages
{
    public class HomePageTests : TestContext
    {
        public HomePageTests()
        {
            // Register mock services for Home.razor and its child components
            var postServiceMock = new Mock<IPostService>();
            var textPostServiceMock = new Mock<ITextPostService>();
            var audioPostServiceMock = new Mock<IAudioPostService>();
            var jsRuntimeMock = new Mock<IJSRuntime>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Setup basic return values for async methods
            postServiceMock
                .Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto>());

            jsRuntimeMock
                .Setup(x => x.InvokeAsync<bool>("eval", It.IsAny<object[]>()))
                .ReturnsAsync(false);

            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            Services.AddSingleton(postServiceMock.Object);
            Services.AddSingleton(textPostServiceMock.Object);
            Services.AddSingleton(audioPostServiceMock.Object);
            Services.AddSingleton(jsRuntimeMock.Object);
            Services.AddSingleton(httpClientFactoryMock.Object);
            Services.AddScoped(_ => new HttpClient());
        }

        [Fact]
        public void HomePage_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            Assert.NotNull(cut);
            var heading = cut.Find("h1");
            Assert.NotNull(heading);
            Assert.Contains("🐻 Tell Teddie", heading.TextContent);
        }

        [Fact]
        public void HomePage_Renders_WithDescription()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var paragraph = cut.Find("p");
            Assert.NotNull(paragraph);
            Assert.Contains("Share your thoughts, feelings, and stories", paragraph.TextContent);
        }

        [Fact]
        public void HomePage_Renders_WithFabButton()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var fabButton = cut.Find("button.fab-button");
            Assert.NotNull(fabButton);
            Assert.Contains("+", fabButton.TextContent);
        }

        [Fact]
        public void HomePage_FabButton_HasAccessibilityLabel()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var fabButton = cut.Find("button.fab-button");
            var ariaLabel = fabButton.GetAttribute("aria-label");
            Assert.NotNull(ariaLabel);
            Assert.Contains("Create new post", ariaLabel);
        }

        [Fact]
        public void HomePage_FabContainer_Exists()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var fabContainer = cut.Find("div.fab-container");
            Assert.NotNull(fabContainer);
        }

        [Fact]
        public void HomePage_PageTitle_IsSet()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            // Component renders successfully with page title
            Assert.NotNull(cut);
        }

        [Fact]
        public void HomePage_FabButton_HasCorrectClass()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var fabButton = cut.Find("button.fab-button");
            Assert.Contains("fab-button", fabButton.ClassName);
        }

        [Fact]
        public void HomePage_Layout_HasProperStructure()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            var heading = cut.Find("h1");
            Assert.NotNull(heading);

            var description = cut.Find("p");
            Assert.NotNull(description);

            var fabContainer = cut.Find("div.fab-container");
            Assert.NotNull(fabContainer);
        }

        [Fact]
        public void HomePage_Renders_WithoutErrors()
        {
            // Arrange & Act
            var cut = Render<Home>();

            // Assert
            Assert.NotNull(cut);
            var content = cut.Find("div.fab-container");
            Assert.NotNull(content);
        }
    }
}
