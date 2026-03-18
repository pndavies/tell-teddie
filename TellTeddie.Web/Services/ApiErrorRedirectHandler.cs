using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace TellTeddie.Web.Services
{
    public sealed class ApiErrorRedirectHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigation;
        private readonly IErrorStateService _errorStateService;

        public ApiErrorRedirectHandler(NavigationManager navigation, IErrorStateService errorStateService = null)
        {
            _navigation = navigation;
            _errorStateService = errorStateService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    RedirectToErrorPage();
                }

                return response;
            }
            catch
            {
                RedirectToErrorPage();
                throw;
            }
        }

        private void RedirectToErrorPage()
        {
            if (!_navigation.Uri.EndsWith("/Error500", StringComparison.OrdinalIgnoreCase))
            {
                _errorStateService?.SetError();
                _navigation.NavigateTo("/Error500");
            }
        }
    }
}
