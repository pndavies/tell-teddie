using Azure.Storage.Blobs;
using TellTeddie.Contracts.DTOs;
using TellTeddie.Contracts.Models;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;

namespace TellTeddie.Api.Services
{
    public interface IAudioPostService
    {
        Task<IEnumerable<AudioPostDto>> GetAllAudioPosts();
        Task InsertAudioPost(PostDto postDto, AudioPostDto audioPostDto);
        Task<string> UploadAudioPost(string base64Audio);
        Task<SasBlobUploadInfo> GetAudioUploadUrl();
    }

    public class AudioPostService : IAudioPostService
    {
        private readonly IAudioPostRepository _audioPostRepository;
        private readonly BlobServiceClient _blobServiceClient;

        public AudioPostService(IAudioPostRepository audioPostRepository, BlobServiceClient blobServiceClient)
        {
            _audioPostRepository = audioPostRepository ?? throw new ArgumentNullException(nameof(audioPostRepository));
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        public async Task<IEnumerable<AudioPostDto>> GetAllAudioPosts()
        {
            var audioPosts = await _audioPostRepository.GetAllAudioPosts();
            return audioPosts.Select(audioPost => new AudioPostDto
            {
                PostID = audioPost.PostID,
                AudioPostUrl = audioPost.AudioPostUrl
            });
        }

        public async Task InsertAudioPost(PostDto postDto, AudioPostDto audioPostDto)
        {
            var post = new Post
            {
                MediaType = postDto.MediaType,
                CreatedAt = postDto.CreatedAt,
                ExpiresAt = postDto.ExpiresAt,
                Name = postDto.Name,
                Caption = postDto.Caption
            };

            var audioPost = new AudioPost
            {
                AudioPostUrl = audioPostDto.AudioPostUrl
            };

            await _audioPostRepository.InsertAudioPost(post, audioPost);
        }

        public async Task<string> UploadAudioPost(string base64Audio)
        {
            var audioBytes = Convert.FromBase64String(base64Audio.Split(',')[1]);
            var fileName = $"{Guid.NewGuid()}.webm";

            var container = _blobServiceClient.GetBlobContainerClient("tell-teddie-blobs");
            await container.CreateIfNotExistsAsync();
            var blobClient = container.GetBlobClient(fileName);

            using var ms = new MemoryStream(audioBytes);
            await blobClient.UploadAsync(ms);

            return blobClient.Uri.ToString();
        }

        public Task<SasBlobUploadInfo> GetAudioUploadUrl()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("tell-teddie-blobs");
            containerClient.CreateIfNotExists();

            var blobName = $"{Guid.NewGuid()}.webm";
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5),
                ContentType = "audio/webm"
            };

            sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Create | Azure.Storage.Sas.BlobSasPermissions.Write);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            var result = new SasBlobUploadInfo
            {
                SasUrl = sasUri.ToString(),
                BlobUrl = blobClient.Uri.ToString(),
                Expiry = sasBuilder.ExpiresOn
            };

            return Task.FromResult(result);
        }
    }
}
