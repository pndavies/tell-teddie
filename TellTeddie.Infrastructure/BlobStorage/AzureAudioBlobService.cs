using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TellTeddie.Infrastructure.BlobStorage
{
    public interface IAzureAudioBlobService
    {
        Task DeleteAudioBlob(string blobUrl);
    }
    public class AzureAudioBlobService : IAzureAudioBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureAudioBlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public Task DeleteAudioBlob(string blobUrl)
        {
            var uri = new Uri(blobUrl);

            string blobContainerName = uri.Segments[1].TrimEnd('/');
            string blobName = string.Join("", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            var deleted = blobClient.DeleteIfExistsAsync();
            return deleted;
        }
    }
}
