using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TellTeddie.Web.Pages.Errors;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Pages
{
    // Custom test implementation of NavigationManager for testing
    public class TestNavigationManager : NavigationManager
    {
        private string _uri;
        private readonly string _baseUri = "http://localhost:7223/";

        public TestNavigationManager(string initialUri = "http://localhost:7223/Error500")
        {
            _uri = initialUri;
            Initialize(_baseUri, initialUri);
        }

        public void SetUri(string uri)
        {
            _uri = uri;
            Uri = uri;
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            // Convert relative URIs to absolute URIs
            if (uri.StartsWith("/"))
            {
                _uri = new Uri(new Uri(_baseUri), uri).ToString();
            }
            else
            {
                _uri = uri;
            }
            Uri = _uri;
        }
    }

    public class Error404PageTests : TestContext
    {
        [Fact]
        public void Error404Page_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            Assert.NotNull(cut);
            var title = cut.Find("h1.fof-title");
            Assert.NotNull(title);
            Assert.Contains("404", title.TextContent);
        }

        [Fact]
        public void Error404Page_Renders_WithSubtitle()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var subtitle = cut.Find("h2.fof-subtitle");
            Assert.NotNull(subtitle);
            Assert.Contains("Are you looking for something?", subtitle.TextContent);
        }

        [Fact]
        public void Error404Page_Renders_WithImage()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var image = cut.Find("img.royal-pup");
            Assert.NotNull(image);
            Assert.Equal("assets/images/IMG_3716.png", image.GetAttribute("src"));
            Assert.Equal("Gracious Princess Theodora", image.GetAttribute("alt"));
        }

        [Fact]
        public void Error404Page_Renders_ReturnHomeButton()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var button = cut.Find("button.btn-home");
            Assert.NotNull(button);
            Assert.Contains("Return Home", button.TextContent);
        }

        [Fact]
        public void Error404Page_InitialCountdown_ShowsTen()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var countdown = cut.Find("div.countdown");
            Assert.NotNull(countdown);
            Assert.Contains("10", countdown.TextContent);
        }

        [Fact]
        public void Error404Page_CatchAllRoute_AcceptsPageRoute()
        {
            // Arrange & Act
            var cut = Render<Error404>(parameters => 
                parameters.Add(p => p.PageRoute, "invalid/nested/path"));

            // Assert
            Assert.NotNull(cut);
            var content = cut.Find("div.fof-content");
            Assert.NotNull(content);
        }

        [Fact]
        public void Error404Page_Renders_WithEmojis()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var emojis = cut.FindAll("div.emoji-float");
            Assert.NotEmpty(emojis);
            Assert.True(emojis.Count >= 4, "Should have at least 4 floating emojis");
        }

        [Fact]
        public void Error404Page_Renders_RedirectNotice()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var notice = cut.Find("div.redirect-notice");
            Assert.NotNull(notice);
            Assert.Contains("magically transported", notice.TextContent);
        }

        [Fact]
        public void Error404Page_Renders_WithError404Content()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var content = cut.Find("div.fof-content");
            Assert.NotNull(content);
            var message = cut.Find("p.fof-message");
            Assert.NotNull(message);
            Assert.Contains("Even royalty 👑 can get lost sometimes", message.TextContent);
        }

        [Fact]
        public void Error404Page_Renders_WithoutErrors()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert - Component should render without errors
            Assert.NotNull(cut);
            var titleElement = cut.Find("h1.fof-title");
            Assert.NotNull(titleElement);
        }

        [Fact]
        public void Error404Page_Renders_AllUiElements()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert - Verify all major UI elements exist
            var container = cut.Find("div.fof-container");
            Assert.NotNull(container);

            var title = cut.Find("h1.fof-title");
            Assert.NotNull(title);

            var subtitle = cut.Find("h2.fof-subtitle");
            Assert.NotNull(subtitle);

            var message = cut.Find("p.fof-message");
            Assert.NotNull(message);

            var button = cut.Find("button.btn-home");
            Assert.NotNull(button);

            var countdown = cut.Find("div.countdown");
            Assert.NotNull(countdown);
        }

        [Fact]
        public void Error404Page_Error404Message_ContainsFriendlyText()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var message = cut.Find("p.fof-message");
            Assert.Contains("kingdom", message.TextContent);
            Assert.Contains("hideaway", message.TextContent);
        }

        [Fact]
        public void Error404Page_RedirectNotice_ContainsCountdown()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert
            var notice = cut.Find("div.redirect-notice");
            var countdown = notice.QuerySelector("div.countdown");
            Assert.NotNull(countdown);
            Assert.Contains("10", countdown.TextContent);
        }

        [Fact]
        public void Error404Page_Layout_HasProperStructure()
        {
            // Arrange & Act
            var cut = Render<Error404>();

            // Assert - Verify layout hierarchy
            var Error404Container = cut.Find("div.fof-container");
            var Error404Content = Error404Container.QuerySelector("div.fof-content");
            Assert.NotNull(Error404Content);

            var image = Error404Content.QuerySelector("img.royal-pup");
            Assert.NotNull(image);

            var actionButtons = Error404Content.QuerySelector("div.action-buttons");
            Assert.NotNull(actionButtons);
        }
    }

    public class Error500PageTests : TestContext
    {
        private IErrorStateService _errorStateService;

        public Error500PageTests()
        {
            // Register required services for ErrorPanel
            Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
            
            // Register ErrorStateService
            _errorStateService = new ErrorStateService();
            Services.AddSingleton(_errorStateService);
            
            // Register NavigationManager with test implementation
            Services.AddSingleton<NavigationManager>(new TestNavigationManager());
            
            // Mock IConfiguration
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.Value).Returns("mailto:test@test.com");
            
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetSection("ErrorReporting:IssueUrlTemplate")).Returns(configurationSection.Object);
            configuration.Setup(c => c["ErrorReporting:IssueUrlTemplate"]).Returns("mailto:test@test.com");
            Services.AddSingleton(configuration.Object);
        }

        /// <summary>
        /// Helper method to render Error500 with error state set (for testing the error UI)
        /// </summary>
        private IRenderedComponent<Error500> RenderWithError()
        {
            _errorStateService.SetError();
            return Render<Error500>();
        }

        [Fact]
        public void Error500Page_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            Assert.NotNull(cut);
            var title = cut.Find("h1.error-title");
            Assert.NotNull(title);
            Assert.Contains("500", title.TextContent);
        }

        [Fact]
        public void Error500Page_Renders_WithSubtitle()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var subtitle = cut.Find("h2.error-subtitle");
            Assert.NotNull(subtitle);
            Assert.Contains("Something went wrong", subtitle.TextContent);
        }

        [Fact]
        public void Error500Page_Renders_WithImage()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var image = cut.Find("img.royal-pup");
            Assert.NotNull(image);
            Assert.Equal("assets/images/princess_tongue-nbg.png", image.GetAttribute("src"));
            Assert.Equal("Teddie", image.GetAttribute("alt"));
        }

        [Fact]
        public void Error500Page_Renders_ErrorMessage()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var message = cut.Find("p.error-message");
            Assert.NotNull(message);
            Assert.Contains("Teddie had a little tumble", message.TextContent);
        }

        [Fact]
        public void Error500Page_Renders_WithEmojis()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var emojis = cut.FindAll("div.emoji-float");
            Assert.NotEmpty(emojis);
            Assert.Equal(5, emojis.Count);
        }

        [Fact]
        public void Error500Page_Renders_ErrorPanelWrapper()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var wrapper = cut.Find("div.error-panel-wrapper");
            Assert.NotNull(wrapper);
        }

        [Fact]
        public void Error500Page_Renders_ErrorContainer()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var container = cut.Find("div.error-container");
            Assert.NotNull(container);
        }

        [Fact]
        public void Error500Page_Renders_ErrorContent()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var content = cut.Find("div.error-content");
            Assert.NotNull(content);
        }

        [Fact]
        public void Error500Page_Renders_AllUiElements()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert - Verify all major UI elements exist
            var container = cut.Find("div.error-container");
            Assert.NotNull(container);

            var title = cut.Find("h1.error-title");
            Assert.NotNull(title);

            var subtitle = cut.Find("h2.error-subtitle");
            Assert.NotNull(subtitle);

            var message = cut.Find("p.error-message");
            Assert.NotNull(message);

            var image = cut.Find("img.royal-pup");
            Assert.NotNull(image);
        }

        [Fact]
        public void Error500Page_ErrorMessage_ContainsFriendlyText()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert
            var message = cut.Find("p.error-message");
            Assert.Contains("tumble", message.TextContent);
            Assert.Contains("report", message.TextContent);
        }

        [Fact]
        public void Error500Page_Layout_HasProperStructure()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert - Verify layout hierarchy
            var container = cut.Find("div.error-container");
            var content = container.QuerySelector("div.error-content");
            Assert.NotNull(content);

            var image = content.QuerySelector("img.royal-pup");
            Assert.NotNull(image);

            var panelWrapper = content.QuerySelector("div.error-panel-wrapper");
            Assert.NotNull(panelWrapper);
        }

        [Fact]
        public void Error500Page_Renders_WithoutErrors()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert - Component should render without errors
            Assert.NotNull(cut);
            var titleElement = cut.Find("h1.error-title");
            Assert.NotNull(titleElement);
        }

        [Fact]
        public void Error500Page_PageTitle_IsSet()
        {
            // Arrange & Act
            var cut = RenderWithError();

            // Assert - PageTitle component should be present
            Assert.NotNull(cut);
        }

        [Fact]
        public void Error500Page_Redirects_To_404_When_Accessed_Directly()
        {
            // Arrange - Do NOT set the error state to simulate direct access
            var navigationManager = Services.GetRequiredService<NavigationManager>() as TestNavigationManager;
            var initialUri = navigationManager.Uri;

            // Act
            var cut = Render<Error500>();

            // Assert - Should be redirected to 404 (catch-all page)
            Assert.NotEqual(initialUri, navigationManager.Uri);
            Assert.Contains("/404", navigationManager.Uri);
        }

        [Fact]
        public void Error500Page_Renders_When_Error_Has_Occurred()
        {
            // Arrange - Set error state to simulate an actual error
            _errorStateService.SetError();
            var navigationManager = Services.GetRequiredService<NavigationManager>() as TestNavigationManager;
            var initialUri = navigationManager.Uri;

            // Act
            var cut = Render<Error500>();

            // Assert - Should remain on Error500 page
            Assert.Equal(initialUri, navigationManager.Uri);
            var container = cut.Find("div.error-container");
            Assert.NotNull(container);
        }
    }
}
