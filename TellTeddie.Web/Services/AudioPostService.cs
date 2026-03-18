using TellTeddie.Contracts.DTOs;

namespace TellTeddie.Web.Services
{
    public interface IAudioPostService
    {
        Task<List<AudioPostDto>> GetAudioPosts();
        Task InsertAudioPost(PostDto postDto, AudioPostDto audioPostDto);
    }
    public class AudioPostService : IAudioPostService
    {
        private readonly HttpClient _httpClient;
        public AudioPostService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TellTeddieApi");
        }

        public async Task<List<AudioPostDto>> GetAudioPosts()
        {
            var response = await _httpClient.GetAsync("api/Post/GetAllAudioPosts");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AudioPostDto>>();
            }
            else
            {
                throw new HttpRequestException($"Error fetching audio posts: {response.ReasonPhrase}");
            }
        }

        public async Task InsertAudioPost(PostDto postDto, AudioPostDto audioPostDto)
        {
            var insertPostAndAudioPost = new InsertAudioPostDto
            {
                Post = postDto,
                AudioPost = audioPostDto
            };

            var response = await _httpClient.PostAsJsonAsync("api/Post/InsertAudioPost", insertPostAndAudioPost);
            response.EnsureSuccessStatusCode();
        }
    }
}
