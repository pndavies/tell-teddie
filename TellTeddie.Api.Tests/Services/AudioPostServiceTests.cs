using Azure.Storage.Blobs;
using Moq;
using TellTeddie.Api.Services;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;
using Xunit;

namespace TellTeddie.Api.Tests.Services
{
    public class AudioPostServiceTests
    {
        private readonly Mock<IAudioPostRepository> _mockAudioPostRepository;
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
        private readonly AudioPostService _systemUnderTest;

        public AudioPostServiceTests()
        {
            _mockAudioPostRepository = new Mock<IAudioPostRepository>();
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _systemUnderTest = new AudioPostService(_mockAudioPostRepository.Object, _mockBlobServiceClient.Object);
        }

        #region GetAllAudioPosts Tests

        [Fact]
        public async Task GetAllAudioPosts_WhenNoPostsExist_ReturnsEmptyList()
        {
            // Arrange
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            var result = await _systemUnderTest.GetAllAudioPosts();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockAudioPostRepository.Verify(x => x.GetAllAudioPosts(), Times.Once);
        }

        [Fact]
        public async Task GetAllAudioPosts_WhenPostsExist_ReturnsMappedDtos()
        {
            // Arrange
            var audioPosts = new List<AudioPost>
            {
                new AudioPost { PostID = 1, AudioPostUrl = "https://blob.com/audio1.webm" },
                new AudioPost { PostID = 2, AudioPostUrl = "https://blob.com/audio2.webm" },
                new AudioPost { PostID = 3, AudioPostUrl = "https://blob.com/audio3.webm" }
            };
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(audioPosts);

            // Act
            var result = await _systemUnderTest.GetAllAudioPosts();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal(1, resultList[0].PostID);
            Assert.Equal("https://blob.com/audio1.webm", resultList[0].AudioPostUrl);
            Assert.Equal(2, resultList[1].PostID);
            Assert.Equal("https://blob.com/audio2.webm", resultList[1].AudioPostUrl);
            Assert.Equal(3, resultList[2].PostID);
            Assert.Equal("https://blob.com/audio3.webm", resultList[2].AudioPostUrl);
            _mockAudioPostRepository.Verify(x => x.GetAllAudioPosts(), Times.Once);
        }

        [Fact]
        public async Task GetAllAudioPosts_WhenSinglePostExists_ReturnsMappedDto()
        {
            // Arrange
            var audioPosts = new List<AudioPost>
            {
                new AudioPost { PostID = 1, AudioPostUrl = "https://blob.com/single.webm" }
            };
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(audioPosts);

            // Act
            var result = await _systemUnderTest.GetAllAudioPosts();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].PostID);
            Assert.Equal("https://blob.com/single.webm", resultList[0].AudioPostUrl);
        }

        #endregion

        #region InsertAudioPost Tests

        [Fact]
        public async Task InsertAudioPost_WithValidData_CallsRepositoryOnce()
        {
            // Arrange
            var postDto = new PostDto
            {
                MediaType = "AUDIO",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Name = "Audio Poster",
                Caption = "Audio Caption"
            };
            var audioPostDto = new AudioPostDto
            {
                AudioPostUrl = "https://blob.com/test-audio.webm"
            };

            // Act
            await _systemUnderTest.InsertAudioPost(postDto, audioPostDto);

            // Assert
            _mockAudioPostRepository.Verify(
                x => x.InsertAudioPost(
                    It.Is<Post>(p =>
                        p.MediaType == "AUDIO" &&
                        p.CreatedAt == postDto.CreatedAt &&
                        p.ExpiresAt == postDto.ExpiresAt &&
                        p.Name == "Audio Poster" &&
                        p.Caption == "Audio Caption"),
                    It.Is<AudioPost>(a => a.AudioPostUrl == "https://blob.com/test-audio.webm")),
                Times.Once);
        }

        [Fact]
        public async Task InsertAudioPost_PreservesAllPostProperties()
        {
            // Arrange
            var createdDate = DateTime.UtcNow;
            var expiryDate = createdDate.AddHours(24);
            var postDto = new PostDto
            {
                MediaType = "AUDIO",
                CreatedAt = createdDate,
                ExpiresAt = expiryDate,
                Name = "Complete Audio",
                Caption = "Complete Caption"
            };
            var audioPostDto = new AudioPostDto
            {
                AudioPostUrl = "https://blob.com/complete-audio.webm"
            };

            // Act
            await _systemUnderTest.InsertAudioPost(postDto, audioPostDto);

            // Assert
            _mockAudioPostRepository.Verify(
                x => x.InsertAudioPost(
                    It.Is<Post>(p =>
                        p.MediaType == "AUDIO" &&
                        p.CreatedAt == createdDate &&
                        p.ExpiresAt == expiryDate &&
                        p.Name == "Complete Audio" &&
                        p.Caption == "Complete Caption"),
                    It.Is<AudioPost>(a => a.AudioPostUrl == "https://blob.com/complete-audio.webm")),
                Times.Once);
        }

        #endregion
    }
}
