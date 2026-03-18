using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Web.Services;

namespace TellTeddie.Web.Tests.Services
{
    public class PostServiceTests
    {
        private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://api/") };
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("TellTeddieApi")).Returns(client);
            return factoryMock.Object;
        }

        [Fact]
        public async Task GetAllPostsAsync_ReturnsList_WhenResponseIsSuccessful()
        {
            // Arrange
            var created = DateTime.Now;
            var expires = DateTime.UtcNow.AddDays(24);

            var payload = new List<PostDto>
            {
                new PostDto
                {
                    PostID = 1,
                    MediaType = "AUDIO",
                    CreatedAt = created,
                    ExpiresAt = expires,
                    TextPost = new TextPostDto
                    {
                        PostID = 1,
                        TextBody = "Testing the service"
                    },
                    AudioPost = new AudioPostDto
                    {
                        PostID= 1,
                        AudioPostUrl = "someurl.com"
                    }
                }
            };
            var json = JsonSerializer.Serialize(payload);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new PostService(factory);

            // Act
            var result = await service.GetAllPosts();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].PostID);
            Assert.Equal("AUDIO", result[0].MediaType);
            Assert.Equal("AUDIO", result[0].MediaType);
            Assert.Equal(created, result[0].CreatedAt);
            Assert.Equal(expires, result[0].ExpiresAt);
            Assert.Equal(1, result[0].TextPost!.PostID);
            Assert.Equal("Testing the service", result[0].TextPost!.TextBody);
            Assert.Equal(1, result[0].AudioPost!.PostID);
            Assert.Equal("someurl.com", result[0].AudioPost!.AudioPostUrl);
            
            handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}
