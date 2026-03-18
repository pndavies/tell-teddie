using Bunit;
using Microsoft.AspNetCore.Components;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Services
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

    public class ApiErrorRedirectHandlerTests
    {
        private readonly TestNavigationManager _navigationManager;
        private readonly ErrorStateService _errorStateService;
        private readonly ApiErrorRedirectHandler _handler;

        public ApiErrorRedirectHandlerTests()
        {
            _navigationManager = new TestNavigationManager("http://localhost:7223/app");
            _errorStateService = new ErrorStateService();
            _handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService);
        }

        [Fact]
        public async Task SendAsync_Redirects_On_ServerError()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            
            var mockInnerHandler = new MockHttpMessageHandler(response);
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Contains("/Error500", _navigationManager.Uri);
        }

        [Fact]
        public async Task SendAsync_Redirects_On_BadRequest()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            
            var mockInnerHandler = new MockHttpMessageHandler(response);
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Contains("/Error500", _navigationManager.Uri);
        }

        [Fact]
        public async Task SendAsync_Redirects_On_NotFound()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            
            var mockInnerHandler = new MockHttpMessageHandler(response);
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Contains("/Error500", _navigationManager.Uri);
        }

        [Fact]
        public async Task SendAsync_DoesNotRedirect_On_Success()
        {
            // Arrange
            var initialUri = _navigationManager.Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            
            var mockInnerHandler = new MockHttpMessageHandler(response);
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(initialUri, _navigationManager.Uri);
        }

        [Fact]
        public async Task SendAsync_Redirects_On_Exception()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            
            var mockInnerHandler = new MockHttpMessageHandler(new InvalidOperationException("API error"));
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await invoker.SendAsync(request, CancellationToken.None);
            });

            Assert.Contains("/Error500", _navigationManager.Uri);
        }

        [Fact]
        public async Task SendAsync_DoesNotRedirect_When_Already_On_ErrorPage()
        {
            // Arrange
            _navigationManager.SetUri("http://localhost:7223/Error500");
            var initialUri = _navigationManager.Uri;
            
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5029/api/posts");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            
            var mockInnerHandler = new MockHttpMessageHandler(response);
            var handler = new ApiErrorRedirectHandler(_navigationManager, _errorStateService)
            {
                InnerHandler = mockInnerHandler
            };

            var invoker = new HttpMessageInvoker(handler);

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(initialUri, _navigationManager.Uri);
        }
    }

    /// <summary>
    /// Mock HTTP message handler for testing ApiErrorRedirectHandler
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _response;
        private readonly Exception? _exception;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public MockHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_exception != null)
            {
                return Task.FromException<HttpResponseMessage>(_exception);
            }

            return Task.FromResult(_response ?? new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
