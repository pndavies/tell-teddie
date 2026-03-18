using TellTeddie.Contracts.DTOs;

namespace TellTeddie.Web.Services
{
    public interface ITextPostService 
    {
        Task<List<TextPostDto>> GetTextPosts();
        Task InsertTextPost(PostDto postDto, TextPostDto textPostDto);
    }
    public class TextPostService : ITextPostService
    {
        private readonly HttpClient _httpClient;
        public TextPostService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TellTeddieApi");
        }

        public async Task<List<TextPostDto>> GetTextPosts()
        {
            var response = await _httpClient.GetAsync("api/Post/GetAllTextPosts");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<TextPostDto>>();
            }
            else
            {
                throw new HttpRequestException($"Error fetching text posts: {response.ReasonPhrase}");
            }
        }

        public async Task InsertTextPost(PostDto postDto, TextPostDto textPostDto)
        {
            var insertPostAndTextPost = new InsertTextPostDto
            {
                Post = postDto,
                TextPost = textPostDto
            };

            var response = await _httpClient.PostAsJsonAsync("api/Post/InsertTextPost", insertPostAndTextPost);
            response.EnsureSuccessStatusCode();
        }
    }
}
