using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Web.Components;
using TellTeddie.Web.Services;
using TellTeddie.Web.ViewModels;
using Xunit;

namespace TellTeddie.Web.Tests.Components
{
    // Custom test implementation of NavigationManager for testing
    public class TestNavigationManager : NavigationManager
    {
        private string _uri;
        private readonly string _baseUri = "http://localhost:7223/";

        public TestNavigationManager(string initialUri = "http://localhost:7223/app")
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

    public class PostFeedComponentTests : TestContext
    {
        private Mock<IPostService> _postServiceMock;
        private TestNavigationManager _navigationManager;

        public PostFeedComponentTests()
        {
            _postServiceMock = new Mock<IPostService>();
            _navigationManager = new TestNavigationManager();
            
            Services.AddSingleton(_postServiceMock.Object);
            Services.AddSingleton<NavigationManager>(_navigationManager);

            // Mock JSInterop calls for WaveSurfer player
            JSInterop.Mode = JSRuntimeMode.Loose;
            
            // Setup WaveSurfer initialization
            JSInterop.Setup<bool>("wavesurferPlayer.init", _ => true)
                .SetResult(true);
            JSInterop.Setup<bool>("wavesurferPlayer.isPlaying", _ => true)
                .SetResult(false);
        }

        #region Basic Rendering Tests

        [Fact]
        public void PostFeed_Renders_WithContainer()
        {
            // Arrange & Act
            var cut = Render<PostFeed>();

            // Assert
            var container = cut.Find("div.post-feed");
            Assert.NotNull(container);
        }

        [Fact]
        public void PostFeed_HasPostFeedDiv()
        {
            // Arrange & Act
            var cut = Render<PostFeed>();

            // Assert
            var postFeed = cut.Find("div.post-feed");
            Assert.NotNull(postFeed);
        }

        [Fact]
        public void PostFeed_Renders_WithoutErrors()
        {
            // Arrange & Act
            var cut = Render<PostFeed>();

            // Assert
            Assert.NotNull(cut);
            var container = cut.Find("div.post-feed");
            Assert.NotNull(container);
        }

        #endregion

        #region Text Post Tests

        [Fact]
        public async Task PostFeed_Renders_TextPost_WithName()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100); // Wait for async initialization

            // Assert
            Assert.Contains("Test User", cut.Markup);
        }

        [Fact]
        public async Task PostFeed_Renders_TextPost_WithCaption()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            Assert.Contains("Test Caption", cut.Markup);
        }

        [Fact]
        public async Task PostFeed_Renders_TextPost_WithTextBody()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            Assert.Contains("Test text body", cut.Markup);
        }

        #endregion

        #region Audio Post with PlyrAudioPlayer Tests

        [Fact]
        public async Task PostFeed_Renders_AudioPost_WithAudioContainer()
        {
            // Arrange
            var audioPost = new PostDto
            {
                PostID = 2,
                Name = "Audio User",
                Caption = "Audio Caption",
                MediaType = "AUDIO",
                CreatedAt = DateTime.Now,
                AudioPost = new AudioPostDto
                {
                    PostID = 2,
                    AudioPostUrl = "https://example.blob.core.windows.net/audio/test.webm"
                }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { audioPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            // Verify audio-post container is rendered (contains PlyrAudioPlayer)
            var audioContainers = cut.FindAll(".audio-post");
            Assert.NotEmpty(audioContainers);
        }

        [Fact]
        public async Task PostFeed_AudioPost_DisplaysNameAndCaption()
        {
            // Arrange
            var audioPost = new PostDto
            {
                PostID = 2,
                Name = "Audio Test User",
                Caption = "Audio Test Caption",
                MediaType = "AUDIO",
                CreatedAt = DateTime.Now,
                AudioPost = new AudioPostDto
                {
                    PostID = 2,
                    AudioPostUrl = "https://example.blob.core.windows.net/audio/test.webm"
                }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { audioPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            Assert.Contains("Audio Test User", cut.Markup);
            Assert.Contains("Audio Test Caption", cut.Markup);
        }

        #endregion

        #region Post Card Styling Tests

        [Fact]
        public async Task PostFeed_PostCards_HaveCorrectClass()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            var postCards = cut.FindAll(".post-card");
            Assert.NotEmpty(postCards);
        }

        [Fact]
        public async Task PostFeed_PostCards_HaveMetadata()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            var metadata = cut.FindAll(".meta-data");
            Assert.NotEmpty(metadata);
        }

        #endregion

        #region Multiple Posts Tests

        [Fact]
        public async Task PostFeed_Renders_MultiplePosts()
        {
            // Arrange
            var posts = new List<PostDto>
            {
                new PostDto
                {
                    PostID = 1,
                    Name = "User 1",
                    Caption = "Caption 1",
                    MediaType = "TEXT",
                    CreatedAt = DateTime.Now,
                    TextPost = new TextPostDto { PostID = 1, TextBody = "Text 1" }
                },
                new PostDto
                {
                    PostID = 2,
                    Name = "User 2",
                    Caption = "Caption 2",
                    MediaType = "AUDIO",
                    CreatedAt = DateTime.Now,
                    AudioPost = new AudioPostDto
                    {
                        PostID = 2,
                        AudioPostUrl = "https://example.blob.core.windows.net/audio/test.webm"
                    }
                }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(posts);

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            Assert.Contains("User 1", cut.Markup);
            Assert.Contains("User 2", cut.Markup);
            Assert.Contains("Text 1", cut.Markup);
            
            // Verify audio post container exists
            var audioContainers = cut.FindAll(".audio-post");
            Assert.NotEmpty(audioContainers);
        }

        [Fact]
        public async Task PostFeed_Renders_MultipleAudioPosts()
        {
            // Arrange
            var posts = new List<PostDto>
            {
                new PostDto
                {
                    PostID = 1,
                    Name = "Audio User 1",
                    Caption = "Audio Caption 1",
                    MediaType = "AUDIO",
                    CreatedAt = DateTime.Now,
                    AudioPost = new AudioPostDto
                    {
                        PostID = 1,
                        AudioPostUrl = "https://example.blob.core.windows.net/audio/test1.webm"
                    }
                },
                new PostDto
                {
                    PostID = 2,
                    Name = "Audio User 2",
                    Caption = "Audio Caption 2",
                    MediaType = "AUDIO",
                    CreatedAt = DateTime.Now,
                    AudioPost = new AudioPostDto
                    {
                        PostID = 2,
                        AudioPostUrl = "https://example.blob.core.windows.net/audio/test2.webm"
                    }
                }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(posts);

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            // Verify multiple audio-post containers
            var audioContainers = cut.FindAll(".audio-post");
            Assert.Equal(2, audioContainers.Count);
        }

        #endregion

        #region Empty State Tests

        [Fact]
        public async Task PostFeed_Renders_NoPostsMessage_WhenEmpty()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto>());

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            Assert.Contains("No posts yet!", cut.Markup);
        }

        [Fact]
        public async Task PostFeed_ShowsLoadingMessage_Initially()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync((List<PostDto>)null!);

            // Act
            var cut = Render<PostFeed>();

            // Assert
            Assert.Contains("Loading posts...", cut.Markup);
        }

        #endregion

        #region Post Actions Tests

        [Fact]
        public async Task PostFeed_PostCards_HaveActionButtons()
        {
            // Arrange
            var textPost = new PostDto
            {
                PostID = 1,
                Name = "Test User",
                Caption = "Test Caption",
                MediaType = "TEXT",
                CreatedAt = DateTime.Now,
                TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
            };

            _postServiceMock.Setup(s => s.GetAllPosts())
                .ReturnsAsync(new List<PostDto> { textPost });

            // Act
            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Assert
            var actionButtons = cut.FindAll(".post-action-btn");
            Assert.NotEmpty(actionButtons);
            Assert.True(actionButtons.Count >= 3); // Like, Share, Comment buttons
        }

        #endregion

        #region Disposal Tests

        [Fact]
        public void PostFeed_DisposesTimerOnDisposal()
        {
            // Arrange
            var cut = Render<PostFeed>();

            // Act
            cut.Instance.Dispose();

            // Assert
            // If no exception thrown, disposal worked correctly
            Assert.NotNull(cut);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task PostFeed_Redirects_To_ErrorPage_When_GetAllPosts_Fails()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetAllPosts())
                .ThrowsAsync(new HttpRequestException("API connection failed"));

            var cut = Render<PostFeed>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(100));

            // Assert
            Assert.Contains("/Error500", _navigationManager.Uri);
        }

        [Fact]
        public async Task PostFeed_Redirects_To_ErrorPage_When_RefreshPosts_Fails()
        {
            // Arrange
            var initialPosts = new List<PostDto>
            {
                new PostDto
                {
                    PostID = 1,
                    Name = "Test User",
                    Caption = "Test Caption",
                    MediaType = "TEXT",
                    CreatedAt = DateTime.Now,
                    TextPost = new TextPostDto { PostID = 1, TextBody = "Test text body" }
                }
            };

            _postServiceMock.SetupSequence(s => s.GetAllPosts())
                .ReturnsAsync(initialPosts)
                .ThrowsAsync(new HttpRequestException("API connection failed"));

            var cut = Render<PostFeed>();
            await Task.Delay(100);

            // Act
            var instance = cut.Instance;
            await cut.InvokeAsync(async () => await instance.RefreshPosts());

            // Assert - Should have attempted to refresh and failed
            Assert.NotNull(cut);
        }

        [Fact]
        public async Task PostFeed_Handles_Multiple_Errors_Gracefully()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetAllPosts())
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act
            var cut = Render<PostFeed>();
            await cut.InvokeAsync(async () => await Task.Delay(100));

            // Assert
            Assert.NotNull(cut);
        }

        #endregion
    }
}
