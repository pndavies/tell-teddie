using Microsoft.AspNetCore.Mvc;
using TellTeddie.Api.Services;
using TellTeddie.Contracts.DTOs;

namespace TellTeddie.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostFeedService _postFeedService;
        private readonly ITextPostService _textPostService;
        private readonly IAudioPostService _audioPostService;

        public PostController(IPostFeedService postFeedService, ITextPostService textPostService, IAudioPostService audioPostService)
        {
            _postFeedService = postFeedService ?? throw new ArgumentNullException(nameof(postFeedService));
            _textPostService = textPostService ?? throw new ArgumentNullException(nameof(textPostService));
            _audioPostService = audioPostService ?? throw new ArgumentNullException(nameof(audioPostService));
        }

        [HttpGet("GetAllPosts")]
        public async Task<IEnumerable<PostDto>> GetAllPosts()
        {
            return await _postFeedService.GetAllPostsForFeed();
        }

        [HttpGet("GetAllTextPosts")]
        public async Task<IEnumerable<TextPostDto>> GetAllTextPosts()
        {
            return await _textPostService.GetAllTextPosts();
        }

        [HttpPost("InsertTextPost")]
        public async Task<IActionResult> InsertTextPost([FromBody] InsertTextPostDto textPostDto)
        {
            await _textPostService.InsertTextPost(textPostDto.Post, textPostDto.TextPost);
            return Ok();
        }

        [HttpGet("GetAllAudioPosts")]
        public async Task<IEnumerable<AudioPostDto>> GetAllAudioPosts()
        {
            return await _audioPostService.GetAllAudioPosts();
        }

        [HttpPost("InsertAudioPost")]
        public async Task<IActionResult> InsertAudioPost([FromBody] InsertAudioPostDto audioPostDto)
        {
            await _audioPostService.InsertAudioPost(audioPostDto.Post, audioPostDto.AudioPost);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadAudioPost([FromBody] string base64Audio)
        {
            var blobUrl = await _audioPostService.UploadAudioPost(base64Audio);
            return Ok(blobUrl);
        }

        [HttpGet("GetAudioUploadUrl")]
        public async Task<IActionResult> GetAudioUploadUrl()
        {
            var result = await _audioPostService.GetAudioUploadUrl();
            return Ok(result);
        }
    }
}
