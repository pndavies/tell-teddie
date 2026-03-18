using Microsoft.Extensions.Configuration;

namespace TellTeddie.Web.Services
{
    /// <summary>
    /// Service for managing Coming Soon mode logic and determining when to show the coming soon page.
    /// </summary>
    public interface IComingSoonService
    {
        /// <summary>
        /// Determines if the application should display the coming soon page.
        /// </summary>
        /// <returns>True if coming soon mode is active, false otherwise.</returns>
        bool IsComingSoonMode();
    }

    public class ComingSoonService : IComingSoonService
    {
        private readonly IConfiguration _configuration;
        private const string DEFAULT_API_ADDRESS = "http://localhost:5029";
        private const string COMING_SOON_MODE = "coming-soon";

        public ComingSoonService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Determines if the application should display the coming soon page.
        /// Returns true if ApiBaseAddress is not configured, empty, or explicitly set to "coming-soon".
        /// </summary>
        public bool IsComingSoonMode()
        {
            var apiBaseAddress = _configuration["ApiBaseAddress"];
            
            // Coming soon mode is active when:
            // 1. ApiBaseAddress is null
            // 2. ApiBaseAddress is empty string
            // 3. ApiBaseAddress is explicitly set to "coming-soon"
            return string.IsNullOrEmpty(apiBaseAddress) || apiBaseAddress == COMING_SOON_MODE;
        }
    }
}
