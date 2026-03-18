using Microsoft.AspNetCore.Mvc;
using Moq;
using TellTeddie.Api.Controllers;
using TellTeddie.Api.Services;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Contracts.Models;
using TellTeddie.Core.DomainModels;
using Xunit;

namespace TellTeddie.Api.Tests
{
    public class PostControllerTests
    {
        private readonly Mock<IPostFeedService> _mockPostFeedService;
        private readonly Mock<ITextPostService> _mockTextPostService;
        private readonly Mock<IAudioPostService> _mockAudioPostService;
        private readonly PostController _controller;

        public PostControllerTests()
        {
            _mockPostFeedService = new Mock<IPostFeedService>();
            _mockTextPostService = new Mock<ITextPostService>();
            _mockAudioPostService = new Mock<IAudioPostService>();
            _controller = new PostController(_mockPostFeedService.Object, _mockTextPostService.Object, _mockAudioPostService.Object);
        }

        [Fact]
        public async Task GetAllPostsForFeed_ReturnsMappedDtoList()
        {
            // Arrange
            var postDtos = new List<PostDto>
            {
                new PostDto { PostID = 1, MediaType = "Text", CreatedAt = DateTime.Now, TextPost = new TextPostDto { PostID = 1, TextBody = "Sample Text" } },
                new PostDto { PostID = 2, MediaType = "Audio", CreatedAt = DateTime.Now, AudioPost = new AudioPostDto { PostID = 2, AudioPostUrl = "sample-audio-url" } }
            };
            _mockPostFeedService.Setup(s => s.GetAllPostsForFeed()).ReturnsAsync(postDtos);

            // Act
            var result = await _controller.GetAllPosts();

            // Assert
            var list = Assert.IsAssignableFrom<IEnumerable<PostDto>>(result);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, p => p.MediaType == "Text");
            Assert.Contains(list, p => p.MediaType == "Audio");
            Assert.Contains(list, p => p.TextPost != null && p.TextPost.TextBody == "Sample Text");
            Assert.Contains(list, p => p.AudioPost != null && p.AudioPost.AudioPostUrl == "sample-audio-url");
        }

        [Fact]
        public async Task GetAllTextPosts_ReturnsMappedDtoList()
        {
            // Arrange
            var textPostDtos = new List<TextPostDto>
            {
                new TextPostDto { PostID = 1, TextBody = "Tell" },
                new TextPostDto { PostID = 2, TextBody = "The bear" }
            };
            _mockTextPostService.Setup(s => s.GetAllTextPosts()).ReturnsAsync(textPostDtos);

            // Act
            var result = await _controller.GetAllTextPosts();

            // Assert
            var list = Assert.IsAssignableFrom<IEnumerable<TextPostDto>>(result);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, t => t.TextBody == "Tell");
            Assert.Contains(list, t => t.TextBody == "The bear");
        }

        [Fact]
        public async Task InsertTextPost_CallsServiceOnce()
        {
            // Arrange
            var dto = new InsertTextPostDto
            {
                Post = new PostDto { MediaType = "Text", CreatedAt = DateTime.Now },
                TextPost = new TextPostDto { TextBody = "New Post" }
            };

            // Act
            var result = await _controller.InsertTextPost(dto);

            // Assert
            _mockTextPostService.Verify(s => s.InsertTextPost(
                It.IsAny<PostDto>(),
                It.IsAny<TextPostDto>()), Times.Once);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAllAudioPosts_ReturnsMappedDtoList()
        {
            // Arrange
            var audioPostDtos = new List<AudioPostDto>
            {
                new AudioPostDto { PostID = 1, AudioPostUrl = "tellTeddieUrl" },
                new AudioPostDto { PostID = 2, AudioPostUrl = "tellTeddieUrl1" }
            };
            _mockAudioPostService.Setup(s => s.GetAllAudioPosts()).ReturnsAsync(audioPostDtos);

            // Act
            var result = await _controller.GetAllAudioPosts();

            // Assert
            var list = Assert.IsAssignableFrom<IEnumerable<AudioPostDto>>(result);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, a => a.AudioPostUrl == "tellTeddieUrl");
            Assert.Contains(list, a => a.AudioPostUrl == "tellTeddieUrl1");
        }

        [Fact]
        public async Task InsertAudioPost_CallsServiceOnce()
        {
            // Arrange
            var dto = new InsertAudioPostDto
            {
                Post = new PostDto { MediaType = "Audio", CreatedAt = DateTime.Now },
                AudioPost = new AudioPostDto { AudioPostUrl = "https://example.com/audio.webm" }
            };

            // Act
            var result = await _controller.InsertAudioPost(dto);

            // Assert
            _mockAudioPostService.Verify(s => s.InsertAudioPost(
                It.IsAny<PostDto>(),
                It.IsAny<AudioPostDto>()), Times.Once);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UploadAudioPost_CallsServiceAndReturnsUrl()
        {
            // Arrange
            string base64Audio = "data:audio/webm;base64," + Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 });
            var expectedUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/test.webm";
            _mockAudioPostService.Setup(s => s.UploadAudioPost(It.IsAny<string>())).ReturnsAsync(expectedUrl);

            // Act
            var result = await _controller.UploadAudioPost(base64Audio);

            // Assert
            _mockAudioPostService.Verify(s => s.UploadAudioPost(base64Audio), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUrl = Assert.IsType<string>(okResult.Value);
            Assert.Equal(expectedUrl, returnedUrl);
        }

        [Fact]
        public async Task GetAudioUploadUrl_CallsServiceAndReturnsSasInfo()
        {
            // Arrange
            var sasInfo = new SasBlobUploadInfo
            {
                SasUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/test.webm?sv=2021-06-08&sig=fake",
                BlobUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/test.webm",
                Expiry = DateTimeOffset.UtcNow.AddMinutes(5)
            };
            _mockAudioPostService.Setup(s => s.GetAudioUploadUrl()).ReturnsAsync(sasInfo);

            // Act
            var result = await _controller.GetAudioUploadUrl();

            // Assert
            _mockAudioPostService.Verify(s => s.GetAudioUploadUrl(), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInfo = Assert.IsType<SasBlobUploadInfo>(okResult.Value);
            Assert.Equal(sasInfo.SasUrl, returnedInfo.SasUrl);
            Assert.Equal(sasInfo.BlobUrl, returnedInfo.BlobUrl);
        }
    }
}