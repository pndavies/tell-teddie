using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TellTeddie.Api.Services;
using TellTeddie.Infrastructure.BlobStorage;
using TellTeddie.Infrastructure.Repositories;


namespace TellTeddie.Api.Tests.Services
{
    public class ExpiredPostServiceTests
    {
        private readonly Mock<ITextPostRepository> _mockTextRepo;
        private readonly Mock<IAudioPostRepository> _mockAudioRepo;
        private readonly Mock<IAzureAudioBlobService> _mockBlobService;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceProvider> _mockRootServiceProvider;
        private readonly Mock<IServiceProvider> _mockScopeServiceProvider;

        public ExpiredPostServiceTests()
        {
            _mockTextRepo = new Mock<ITextPostRepository>();
            _mockAudioRepo = new Mock<IAudioPostRepository>();
            _mockBlobService = new Mock<IAzureAudioBlobService>();

            _mockScope = new Mock<IServiceScope>();
            _mockScopeServiceProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockRootServiceProvider = new Mock<IServiceProvider>();

            // Scope.ServiceProvider should return a provider that resolves the repository / blob service
            _mockScopeServiceProvider
                .Setup(p => p.GetService(typeof(ITextPostRepository)))
                .Returns(_mockTextRepo.Object);

            _mockScopeServiceProvider
                .Setup(p => p.GetService(typeof(IAudioPostRepository)))
                .Returns(_mockAudioRepo.Object);

            _mockScopeServiceProvider
                .Setup(p => p.GetService(typeof(IAzureAudioBlobService)))
                .Returns(_mockBlobService.Object);

            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockScopeServiceProvider.Object);

            // Root provider must return an IServiceScopeFactory so CreateScope() extension works
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
            _mockRootServiceProvider
                .Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);
        }

        private static async Task InvokeDeleteExpiredPostsAsync(ExpiredPostService service, CancellationToken token)
        {
            var method = typeof(ExpiredPostService).GetMethod("DeleteExpiredPosts", BindingFlags.Instance | BindingFlags.NonPublic);
            var task = (Task)method.Invoke(service, new object[] { token })!;
            await task;
        }

        [Fact]
        public async Task DeleteExpiredPosts_DeletesTextAndAudioBlobsAndAudioPosts()
        {
            // Arrange
            var expiredUrls = new List<string> { "https://a.blob/1.webm", "https://a.blob/2.webm" };
            _mockTextRepo.Setup(r => r.DeleteExpiredTextPosts()).Returns(Task.CompletedTask);
            _mockAudioRepo.Setup(r => r.GetExpiredAudioUrls()).ReturnsAsync(expiredUrls);
            _mockBlobService.Setup(b => b.DeleteAudioBlob(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockAudioRepo.Setup(r => r.DeleteExpiredAudioPosts()).Returns(Task.CompletedTask);

            var service = new ExpiredPostService(_mockRootServiceProvider.Object);

            // Act
            await InvokeDeleteExpiredPostsAsync(service, CancellationToken.None);

            // Assert
            _mockScopeFactory.Verify(f => f.CreateScope(), Times.Once);
            _mockTextRepo.Verify(r => r.DeleteExpiredTextPosts(), Times.Once);
            _mockAudioRepo.Verify(r => r.GetExpiredAudioUrls(), Times.Once);
            _mockBlobService.Verify(b => b.DeleteAudioBlob("https://a.blob/1.webm"), Times.Once);
            _mockBlobService.Verify(b => b.DeleteAudioBlob("https://a.blob/2.webm"), Times.Once);
            _mockAudioRepo.Verify(r => r.DeleteExpiredAudioPosts(), Times.Once);
        }

        [Fact]
        public async Task DeleteExpiredPosts_WhenBlobDeleteThrows_BreaksLoopAndStillDeletesAudioPosts()
        {
            // Arrange
            var expiredUrls = new List<string> { "https://a.blob/1.webm", "https://a.blob/2.webm", "https://a.blob/3.webm" };
            _mockTextRepo.Setup(r => r.DeleteExpiredTextPosts()).Returns(Task.CompletedTask);
            _mockAudioRepo.Setup(r => r.GetExpiredAudioUrls()).ReturnsAsync(expiredUrls);

            // Throw on first delete, subsequent should not be invoked
            _mockBlobService.Setup(b => b.DeleteAudioBlob("https://a.blob/1.webm"))
                .ThrowsAsync(new InvalidOperationException("fail"));
            _mockBlobService.Setup(b => b.DeleteAudioBlob(It.Is<string>(s => s != "https://a.blob/1.webm")))
                .Returns(Task.CompletedTask);

            _mockAudioRepo.Setup(r => r.DeleteExpiredAudioPosts()).Returns(Task.CompletedTask);

            var service = new ExpiredPostService(_mockRootServiceProvider.Object);

            // Act
            await InvokeDeleteExpiredPostsAsync(service, CancellationToken.None);

            // Assert
            // delete was attempted only for the first url, loop broke afterwards
            _mockBlobService.Verify(b => b.DeleteAudioBlob("https://a.blob/1.webm"), Times.Once);
            _mockBlobService.Verify(b => b.DeleteAudioBlob("https://a.blob/2.webm"), Times.Never);
            _mockBlobService.Verify(b => b.DeleteAudioBlob("https://a.blob/3.webm"), Times.Never);

            // still attempts to delete expired audio posts record
            _mockAudioRepo.Verify(r => r.DeleteExpiredAudioPosts(), Times.Once);
        }

        [Fact]
        public async Task DeleteExpiredPosts_NoExpiredAudioUrls_DoesNotCallBlobService()
        {
            // Arrange
            _mockTextRepo.Setup(r => r.DeleteExpiredTextPosts()).Returns(Task.CompletedTask);
            _mockAudioRepo.Setup(r => r.GetExpiredAudioUrls()).ReturnsAsync(new List<string>());
            _mockAudioRepo.Setup(r => r.DeleteExpiredAudioPosts()).Returns(Task.CompletedTask);

            var service = new ExpiredPostService(_mockRootServiceProvider.Object);

            // Act
            await InvokeDeleteExpiredPostsAsync(service, CancellationToken.None);

            // Assert
            _mockBlobService.Verify(b => b.DeleteAudioBlob(It.IsAny<string>()), Times.Never);
            _mockAudioRepo.Verify(r => r.DeleteExpiredAudioPosts(), Times.Once);
            _mockTextRepo.Verify(r => r.DeleteExpiredTextPosts(), Times.Once);
        }
    }
}
