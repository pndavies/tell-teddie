using Moq;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Api.Services;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;
using Xunit;

namespace TellTeddie.Api.Tests.Services
{
    public class PostFeedServiceTests
    {
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<ITextPostRepository> _mockTextPostRepository;
        private readonly Mock<IAudioPostRepository> _mockAudioPostRepository;
        private readonly PostFeedService _systemUnderTest; 

        public PostFeedServiceTests()
        {
            _mockPostRepository = new Mock<IPostRepository>();
            _mockTextPostRepository = new Mock<ITextPostRepository>();
            _mockAudioPostRepository = new Mock<IAudioPostRepository>();

            _systemUnderTest = new PostFeedService(
                _mockPostRepository.Object,
                _mockTextPostRepository.Object,
                _mockAudioPostRepository.Object
            );
        }

        [Fact]
        public async Task GetAllPostsForFeed_WhenNoPostsExist_ReturnsEmptyList()
        {
            // Arrange
            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(new List<Post>());
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(new List<TextPost>());
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            var result = await _systemUnderTest.GetAllPostsForFeed();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPostsForFeed_WhenTextPostExists_ReturnsPostWithTextPost()
        {
            // Arrange
            var postId = 1;
            var posts = new List<Post>
            {
                new Post
                {
                    PostID = postId,
                    MediaType = "Text",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                }
            };

            var textPosts = new List<TextPost>
            {
                new TextPost
                {
                    PostID = postId,
                    TextBody = "This is a test text post"
                }
            };

            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(posts);
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(textPosts);
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            var result = (await _systemUnderTest.GetAllPostsForFeed()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(postId, result[0].PostID);
            Assert.Equal("Text", result[0].MediaType);
            Assert.NotNull(result[0].TextPost);
            Assert.Equal("This is a test text post", result[0].TextPost.TextBody);
            Assert.Null(result[0].AudioPost);
        }

        [Fact]
        public async Task GetAllPostsForFeed_WhenAudioPostExists_ReturnsPostWithAudioPost()
        {
            // Arrange
            var postId = 2;
            var posts = new List<Post>
            {
                new Post
                {
                    PostID = postId,
                    MediaType = "Audio",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                }
            };

            var audioPosts = new List<AudioPost>
            {
                new AudioPost
                {
                    PostID = postId,
                    AudioPostUrl = "https://blob.storage/audio123.webm"
                }
            };

            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(posts);
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(new List<TextPost>());
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(audioPosts);

            // Act
            var result = (await _systemUnderTest.GetAllPostsForFeed()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(postId, result[0].PostID);
            Assert.Equal("Audio", result[0].MediaType);
            Assert.Null(result[0].TextPost);
            Assert.NotNull(result[0].AudioPost);
            Assert.Equal("https://blob.storage/audio123.webm", result[0].AudioPost.AudioPostUrl);
        }

        [Fact]
        public async Task GetAllPostsForFeed_WhenMultiplePostsExist_ReturnsAllPostsWithCorrectData()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    PostID = 1,
                    MediaType = "Text",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                },
                new Post
                {
                    PostID = 2,
                    MediaType = "Audio",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                },
                new Post
                {
                    PostID = 3,
                    MediaType = "Text",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                }
            };

            var textPosts = new List<TextPost>
            {
                new TextPost { PostID = 1, TextBody = "First text post" },
                new TextPost { PostID = 3, TextBody = "Second text post" }
            };

            var audioPosts = new List<AudioPost>
            {
                new AudioPost { PostID = 2, AudioPostUrl = "https://blob.storage/audio.webm" }
            };

            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(posts);
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(textPosts);
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(audioPosts);

            // Act
            var result = (await _systemUnderTest.GetAllPostsForFeed()).ToList();

            // Assert
            Assert.Equal(3, result.Count);

            // Check first post (Text)
            Assert.Equal(1, result[0].PostID);
            Assert.NotNull(result[0].TextPost);
            Assert.Equal("First text post", result[0].TextPost.TextBody);
            Assert.Null(result[0].AudioPost);

            // Check second post (Audio)
            Assert.Equal(2, result[1].PostID);
            Assert.Null(result[1].TextPost);
            Assert.NotNull(result[1].AudioPost);
            Assert.Equal("https://blob.storage/audio.webm", result[1].AudioPost.AudioPostUrl);

            // Check third post (Text)
            Assert.Equal(3, result[2].PostID);
            Assert.NotNull(result[2].TextPost);
            Assert.Equal("Second text post", result[2].TextPost.TextBody);
            Assert.Null(result[2].AudioPost);
        }

        [Fact]
        public async Task GetAllPostsForFeed_WhenPostHasNoMatchingContent_ReturnsPostWithNullContent()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    PostID = 1,
                    MediaType = "Text",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                }
            };

            // No text or audio posts returned (orphaned post scenario)
            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(posts);
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(new List<TextPost>());
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            var result = (await _systemUnderTest.GetAllPostsForFeed()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].PostID);
            Assert.Null(result[0].TextPost);
            Assert.Null(result[0].AudioPost);
        }

        [Fact]
        public async Task GetAllPostsForFeed_MapsAllPostPropertiesCorrectly()
        {
            // Arrange
            var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var expiresAt = createdAt.AddHours(24);

            var posts = new List<Post>
            {
                new Post
                {
                    PostID = 123,
                    MediaType = "Text",
                    CreatedAt = createdAt,
                    ExpiresAt = expiresAt
                }
            };

            var textPosts = new List<TextPost>
            {
                new TextPost { PostID = 123, TextBody = "Test content" }
            };

            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(posts);
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(textPosts);
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            var result = (await _systemUnderTest.GetAllPostsForFeed()).ToList();

            // Assert
            var dto = result[0];
            Assert.Equal(123, dto.PostID);
            Assert.Equal("Text", dto.MediaType);
            Assert.Equal(createdAt, dto.CreatedAt);
            Assert.Equal(expiresAt, dto.ExpiresAt);
        }

        [Fact]
        public async Task GetAllPostsForFeed_CallsAllRepositories()
        {
            // Arrange
            _mockPostRepository.Setup(x => x.GetAllPosts())
                .ReturnsAsync(new List<Post>());
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(new List<TextPost>());
            _mockAudioPostRepository.Setup(x => x.GetAllAudioPosts())
                .ReturnsAsync(new List<AudioPost>());

            // Act
            await _systemUnderTest.GetAllPostsForFeed();

            // Assert
            _mockPostRepository.Verify(x => x.GetAllPosts(), Times.Once);
            _mockTextPostRepository.Verify(x => x.GetAllTextPosts(), Times.Once);
            _mockAudioPostRepository.Verify(x => x.GetAllAudioPosts(), Times.Once);
        }
    }
}