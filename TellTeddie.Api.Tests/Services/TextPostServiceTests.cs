using Moq;
using TellTeddie.Api.Services;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;
using Xunit;

namespace TellTeddie.Api.Tests.Services
{
    public class TextPostServiceTests
    {
        private readonly Mock<ITextPostRepository> _mockTextPostRepository;
        private readonly TextPostService _systemUnderTest;

        public TextPostServiceTests()
        {
            _mockTextPostRepository = new Mock<ITextPostRepository>();
            _systemUnderTest = new TextPostService(_mockTextPostRepository.Object);
        }

        #region GetAllTextPosts Tests

        [Fact]
        public async Task GetAllTextPosts_WhenNoPostsExist_ReturnsEmptyList()
        {
            // Arrange
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(new List<TextPost>());

            // Act
            var result = await _systemUnderTest.GetAllTextPosts();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockTextPostRepository.Verify(x => x.GetAllTextPosts(), Times.Once);
        }

        [Fact]
        public async Task GetAllTextPosts_WhenPostsExist_ReturnsMappedDtos()
        {
            // Arrange
            var textPosts = new List<TextPost>
            {
                new TextPost { PostID = 1, TextBody = "First text post" },
                new TextPost { PostID = 2, TextBody = "Second text post" },
                new TextPost { PostID = 3, TextBody = "Third text post" }
            };
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(textPosts);

            // Act
            var result = await _systemUnderTest.GetAllTextPosts();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal(1, resultList[0].PostID);
            Assert.Equal("First text post", resultList[0].TextBody);
            Assert.Equal(2, resultList[1].PostID);
            Assert.Equal("Second text post", resultList[1].TextBody);
            Assert.Equal(3, resultList[2].PostID);
            Assert.Equal("Third text post", resultList[2].TextBody);
            _mockTextPostRepository.Verify(x => x.GetAllTextPosts(), Times.Once);
        }

        [Fact]
        public async Task GetAllTextPosts_WhenSinglePostExists_ReturnsMappedDto()
        {
            // Arrange
            var textPosts = new List<TextPost>
            {
                new TextPost { PostID = 1, TextBody = "Single post" }
            };
            _mockTextPostRepository.Setup(x => x.GetAllTextPosts())
                .ReturnsAsync(textPosts);

            // Act
            var result = await _systemUnderTest.GetAllTextPosts();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].PostID);
            Assert.Equal("Single post", resultList[0].TextBody);
        }

        #endregion

        #region InsertTextPost Tests

        [Fact]
        public async Task InsertTextPost_WithValidData_CallsRepositoryOnce()
        {
            // Arrange
            var postDto = new PostDto
            {
                MediaType = "TEXT",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Name = "Test User",
                Caption = "Test Caption"
            };
            var textPostDto = new TextPostDto
            {
                TextBody = "This is a test text post"
            };

            // Act
            await _systemUnderTest.InsertTextPost(postDto, textPostDto);

            // Assert
            _mockTextPostRepository.Verify(
                x => x.InsertTextPost(
                    It.Is<Post>(p =>
                        p.MediaType == "TEXT" &&
                        p.CreatedAt == postDto.CreatedAt &&
                        p.ExpiresAt == postDto.ExpiresAt &&
                        p.Name == "Test User" &&
                        p.Caption == "Test Caption"),
                    It.Is<TextPost>(t => t.TextBody == "This is a test text post")),
                Times.Once);
        }

        [Fact]
        public async Task InsertTextPost_WithAnonymousUser_CreatesPostSuccessfully()
        {
            // Arrange
            var postDto = new PostDto
            {
                MediaType = "TEXT",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Name = "Anonymous",
                Caption = string.Empty
            };
            var textPostDto = new TextPostDto
            {
                TextBody = "Anonymous post"
            };

            // Act
            await _systemUnderTest.InsertTextPost(postDto, textPostDto);

            // Assert
            _mockTextPostRepository.Verify(
                x => x.InsertTextPost(It.IsAny<Post>(), It.IsAny<TextPost>()),
                Times.Once);
        }

       
        [Fact]
        public async Task InsertTextPost_PreservesAllPostProperties()
        {
            // Arrange
            var createdDate = DateTime.UtcNow;
            var expiryDate = createdDate.AddHours(24);
            var postDto = new PostDto
            {
                MediaType = "TEXT",
                CreatedAt = createdDate,
                ExpiresAt = expiryDate,
                Name = "Test User",
                Caption = "Test Caption"
            };
            var textPostDto = new TextPostDto
            {
                TextBody = "Complete text post"
            };

            // Act
            await _systemUnderTest.InsertTextPost(postDto, textPostDto);

            // Assert
            _mockTextPostRepository.Verify(
                x => x.InsertTextPost(
                    It.Is<Post>(p =>
                        p.MediaType == "TEXT" &&
                        p.CreatedAt == createdDate &&
                        p.ExpiresAt == expiryDate &&
                        p.Name == "Test User" &&
                        p.Caption == "Test Caption"),
                    It.Is<TextPost>(t => t.TextBody == "Complete text post")),
                Times.Once);
        }

        #endregion
    }
}
