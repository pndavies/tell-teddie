using AngleSharp.Dom;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Services
{
    public class AudioPostServiceTests
    {
        private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://api/") };
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("TellTeddieApi")).Returns(client);
            return factoryMock.Object;
        }

        [Fact]
       public async Task GetAudioPosts_ReturnsList_WhenResponseIsSuccessful()
        {
            // Arrange
            var payload = new List<AudioPostDto>
            {
                new AudioPostDto { PostID = 1, AudioPostUrl = "https://cdn/test/1.webm" }
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
            var service = new AudioPostService(factory);

            // Act
            var result = await service.GetAudioPosts();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].PostID);
            Assert.Equal("https://cdn/test/1.webm", result[0].AudioPostUrl);
            handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetAudioPosts_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Bad things"
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new AudioPostService(factory);

            // Act / Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAudioPosts());
        }

        [Fact]
        public async Task InsertAudioPost_PostsPayload_AndSucceeds()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
               {
                   Assert.Equal(HttpMethod.Post, req.Method);
                   Assert.EndsWith("api/Post/InsertAudioPost", req.RequestUri.ToString());

                   // optional: inspect JSON body
                   var body = req.Content.ReadFromJsonAsync<InsertAudioPostDto>(cancellationToken: ct).Result;
                   Assert.NotNull(body);
                   Assert.Equal("Audio", body.Post.MediaType);
                   Assert.Equal("https://cdn/test/new.webm", body.AudioPost.AudioPostUrl);

                   return new HttpResponseMessage(HttpStatusCode.Created);
               });

            var factory = CreateHttpClientFactory(handlerMock.Object);
            var service = new AudioPostService(factory);

            var postDto = new PostDto { MediaType = "Audio", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            var audioDto = new AudioPostDto { AudioPostUrl = "https://cdn/test/new.webm" };

            // Act
            await service.InsertAudioPost(postDto, audioDto);

            // Assert - if no exception thrown the test passes; verify handler was invoked
            handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task InsertAudioPost_Throws_WhenResponseIsNotSuccessful()
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
            var service = new AudioPostService(factory);

            var postDto = new PostDto { MediaType = "Audio", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            var audioDto = new AudioPostDto { AudioPostUrl = "https://cdn/test/new.webm" };

            // Act / Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.InsertAudioPost(postDto, audioDto));
        }
    }
}