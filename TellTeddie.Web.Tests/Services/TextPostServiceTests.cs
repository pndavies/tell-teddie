using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Services
{
    public class TextPostServiceTests
    {
        private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://api/") };
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("TellTeddieApi")).Returns(client);
            return factoryMock.Object;
        }

        [Fact]
        public async Task GetTextPostsAsync_ReturnsList_WhenResponseIsSuccessful()
        {
            // Arrange
            var payload = new List<TextPostDto>
            {
                new TextPostDto { PostID = 1, TextBody = "hello" }
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
            var service = new TextPostService(factory);

            // Act
            var result = await service.GetTextPosts();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].PostID);
            Assert.Equal("hello", result[0].TextBody);
            handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetTextPostsAsync_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Server error"
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new TextPostService(factory);

            // Act / Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetTextPosts());
        }

        [Fact]
        public async Task InsertTextPostAsync_PostsPayload_AndSucceeds()
        {
            // Arrange
            var expectedPost = new PostDto { MediaType = "Text", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            var expectedText = new TextPostDto { TextBody = "new post" };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
               {
                   Assert.Equal(HttpMethod.Post, req.Method);
                   Assert.EndsWith("api/Post/InsertTextPost", req.RequestUri.ToString());

                   // Inspect body
                   var bodyJson = req.Content.ReadAsStringAsync(ct).Result;
                   var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                   var body = JsonSerializer.Deserialize<InsertTextPostDto>(bodyJson, opts);
                   Assert.NotNull(body);
                   Assert.Equal(expectedPost.MediaType, body!.Post.MediaType);
                   Assert.Equal(expectedText.TextBody, body.TextPost.TextBody);

                   return new HttpResponseMessage(HttpStatusCode.Created);
               });

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new TextPostService(factory);

            // Act
            await service.InsertTextPost(expectedPost, expectedText);

            // Assert - no exception thrown and handler invoked
            handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task InsertTextPostAsync_Throws_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new TextPostService(factory);

            var postDto = new PostDto { MediaType = "Text", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            var textDto = new TextPostDto { TextBody = "bad" };

            // Act / Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.InsertTextPost(postDto, textDto));
        }
    }
}