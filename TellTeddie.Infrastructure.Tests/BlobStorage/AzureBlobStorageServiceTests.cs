using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using TellTeddie.Infrastructure.BlobStorage;


namespace TellTeddie.Infrastructure.Tests.BlobStorage
{
    public class AzureAudioBlobServiceTests
    {
        [Fact]
        public async Task DeleteAudioBlob_CallsDeleteIfExistsOnCorrectContainerAndBlob()
        {
            // Arrange
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockContainerClient = new Mock<BlobContainerClient>();
            var mockBlobClient = new Mock<BlobClient>();

            var blobUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/path/to/file.webm";
            var uri = new Uri(blobUrl);
            var expectedContainer = uri.Segments[1].TrimEnd('/');
            var expectedBlobName = string.Concat(uri.Segments.Skip(2));

            mockBlobServiceClient
                .Setup(s => s.GetBlobContainerClient(expectedContainer))
                .Returns(mockContainerClient.Object);

            mockContainerClient
                .Setup(c => c.GetBlobClient(expectedBlobName))
                .Returns(mockBlobClient.Object);

            mockBlobClient
                .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>());

            var service = new AzureAudioBlobService(mockBlobServiceClient.Object);

            // Act
            await service.DeleteAudioBlob(blobUrl);

            // Assert
            mockBlobServiceClient.Verify(s => s.GetBlobContainerClient(expectedContainer), Times.Once);
            mockContainerClient.Verify(c => c.GetBlobClient(expectedBlobName), Times.Once);
            mockBlobClient.Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAudioBlob_WhenDeleteThrows_PropagatesException()
        {
            // Arrange
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockContainerClient = new Mock<BlobContainerClient>();
            var mockBlobClient = new Mock<BlobClient>();

            var blobUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/file.webm";
            var uri = new Uri(blobUrl);
            var expectedContainer = uri.Segments[1].TrimEnd('/');
            var expectedBlobName = string.Concat(uri.Segments.Skip(2));

            mockBlobServiceClient
                .Setup(s => s.GetBlobContainerClient(expectedContainer))
                .Returns(mockContainerClient.Object);

            mockContainerClient
                .Setup(c => c.GetBlobClient(expectedBlobName))
                .Returns(mockBlobClient.Object);

            mockBlobClient
                .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("delete failed"));

            var service = new AzureAudioBlobService(mockBlobServiceClient.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAudioBlob(blobUrl));
            mockBlobClient.Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAudioBlob_WithNestedPath_ConstructsBlobNameCorrectly()
        {
            // Arrange
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockContainerClient = new Mock<BlobContainerClient>();
            var mockBlobClient = new Mock<BlobClient>();

            var blobUrl = "https://fake.blob.core.windows.net/tell-teddie-blobs/folder1/folder2/file.webm";
            var uri = new Uri(blobUrl);
            var expectedContainer = uri.Segments[1].TrimEnd('/');
            var expectedBlobName = string.Concat(uri.Segments.Skip(2)); // should preserve internal slashes from segments

            mockBlobServiceClient
                .Setup(s => s.GetBlobContainerClient(expectedContainer))
                .Returns(mockContainerClient.Object);

            // Verify we receive the expected blobName when GetBlobClient is called
            mockContainerClient
                .Setup(c => c.GetBlobClient(It.Is<string>(name => name == expectedBlobName)))
                .Returns(mockBlobClient.Object);

            mockBlobClient
                .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>());

            var service = new AzureAudioBlobService(mockBlobServiceClient.Object);

            // Act
            await service.DeleteAudioBlob(blobUrl);

            // Assert
            mockContainerClient.Verify(c => c.GetBlobClient(expectedBlobName), Times.Once);
            mockBlobClient.Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}