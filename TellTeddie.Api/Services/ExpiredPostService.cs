
using TellTeddie.Infrastructure.BlobStorage;
using TellTeddie.Infrastructure.Repositories;


namespace TellTeddie.Api.Services
{
    public class ExpiredPostService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ExpiredPostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Uncomment for testing - will delete expired posts on startup
            // await DeleteExpiredPosts(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                await DeleteExpiredPosts(stoppingToken);
            }
        }

        private async Task DeleteExpiredPosts(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            
            // Get Repositories
            var textRepository = scope.ServiceProvider.GetRequiredService<ITextPostRepository>();
            var audioRepository = scope.ServiceProvider.GetRequiredService<IAudioPostRepository>();
            var audioBlobService = scope.ServiceProvider.GetRequiredService<IAzureAudioBlobService>();

            // Delete Expired Text Posts
            await textRepository.DeleteExpiredTextPosts();

            // Delete Expired Audio Posts and their blobs
            var expiredAudioPostUrls = await audioRepository.GetExpiredAudioUrls();

            foreach (var url in expiredAudioPostUrls)
            {
                try
                {
                    await audioBlobService.DeleteAudioBlob(url);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            await audioRepository.DeleteExpiredAudioPosts();
        }
    }
}
