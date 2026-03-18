using TellTeddie.Contracts.DTOs;

namespace TellTeddie.Web.Services
{
    public interface IPostService
    {
        Task<List<PostDto>> GetAllPosts();
    }
    public class PostService : IPostService
    {
        private readonly HttpClient _httpClient;
        public PostService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TellTeddieApi");
        }
        public async Task<List<PostDto>> GetAllPosts()
        {
            var response = await _httpClient.GetAsync("api/Post/GetAllPosts");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PostDto>>();
            }
            else
            {
                throw new HttpRequestException($"Error fetching posts: {response.ReasonPhrase}");
            }
        }
    }
}
