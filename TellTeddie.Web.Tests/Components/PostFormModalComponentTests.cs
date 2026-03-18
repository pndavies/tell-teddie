using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using TellTeddie.Web.Components.Forms;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Components
{
    public class PostFormModalComponentTests : TestContext
    {
        private Mock<ITextPostService> _textPostServiceMock;
        private Mock<IAudioPostService> _audioPostServiceMock;
        private Mock<IJSRuntime> _jsRuntimeMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IPostService> _postServiceMock;

        public PostFormModalComponentTests()
        {
            _textPostServiceMock = new Mock<ITextPostService>();
            _audioPostServiceMock = new Mock<IAudioPostService>();
            _jsRuntimeMock = new Mock<IJSRuntime>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _postServiceMock = new Mock<IPostService>();

            // Setup JS Runtime mock
            _jsRuntimeMock
                .Setup(x => x.InvokeAsync<bool>("eval", It.IsAny<object[]>()))
                .ReturnsAsync(false);

            // Setup HttpClientFactory mock
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            Services.AddSingleton(_textPostServiceMock.Object);
            Services.AddSingleton(_audioPostServiceMock.Object);
            Services.AddSingleton(_jsRuntimeMock.Object);
            Services.AddSingleton(_httpClientFactoryMock.Object);
            Services.AddSingleton(_postServiceMock.Object);
            Services.AddScoped(_ => new HttpClient());
        }

        [Fact]
        public void PostFormModal_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var title = cut.Find("h5.modal-title");
            Assert.NotNull(title);
            Assert.Contains("Create a Post", title.TextContent);
        }

        [Fact]
        public void PostFormModal_Renders_WithTabs()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var tabButtons = cut.FindAll("button.nav-link");
            Assert.True(tabButtons.Count >= 2, "Should have at least 2 tabs (Audio and Text)");
        }

        [Fact]
        public void PostFormModal_HasAudioTab()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var tabs = cut.FindAll("button.nav-link");
            var audioTab = tabs.FirstOrDefault(t => t.TextContent.Contains("Audio"));
            Assert.NotNull(audioTab);
        }

        [Fact]
        public void PostFormModal_HasTextTab()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var tabs = cut.FindAll("button.nav-link");
            var textTab = tabs.FirstOrDefault(t => t.TextContent.Contains("Text"));
            Assert.NotNull(textTab);
        }

        [Fact]
        public void PostFormModal_HasCloseButton()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var closeButton = cut.Find("button.btn-close");
            Assert.NotNull(closeButton);
            var ariaLabel = closeButton.GetAttribute("aria-label");
            Assert.NotNull(ariaLabel);
            Assert.Contains("Close", ariaLabel);
        }

        [Fact]
        public void PostFormModal_HasModalDialog()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var dialog = cut.Find("div.modal-dialog");
            Assert.NotNull(dialog);
        }

        [Fact]
        public void PostFormModal_HasModalContent()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var content = cut.Find("div.modal-content");
            Assert.NotNull(content);
        }

        [Fact]
        public void PostFormModal_HasModalHeader()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var header = cut.Find("div.modal-header");
            Assert.NotNull(header);
        }

        [Fact]
        public void PostFormModal_HasModalBody()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var body = cut.Find("div.modal-body");
            Assert.NotNull(body);
        }

        [Fact]
        public void PostFormModal_HasTabContent()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var tabContent = cut.Find("div.tab-content");
            Assert.NotNull(tabContent);
        }

        [Fact]
        public void PostFormModal_Renders_WithoutErrors_WhenVisible()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            Assert.NotNull(cut);
        }

        [Fact]
        public void PostFormModal_HasNavTabs()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var navTabs = cut.Find("ul.nav-tabs");
            Assert.NotNull(navTabs);
        }

        [Fact]
        public void PostFormModal_Tabs_HaveProperRole()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var tabs = cut.FindAll("button.nav-link");
            Assert.True(tabs.Count > 0);
            // Each tab should have role="tab"
            foreach (var tab in tabs)
            {
                var role = tab.GetAttribute("role");
                Assert.Equal("tab", role);
            }
        }

        [Fact]
        public void PostFormModal_HasModalLargeSize()
        {
            // Arrange & Act
            var cut = Render<PostFormModal>(parameters =>
                parameters.Add(p => p.IsVisible, true));

            // Assert
            var dialog = cut.Find("div.modal-dialog");
            Assert.Contains("modal-lg", dialog.ClassName);
        }
    }
}
