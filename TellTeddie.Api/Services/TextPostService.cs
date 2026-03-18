using TellTeddie.Contracts.DTOs;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;

namespace TellTeddie.Api.Services
{
    public interface ITextPostService
    {
        Task<IEnumerable<TextPostDto>> GetAllTextPosts();
        Task InsertTextPost(PostDto postDto, TextPostDto textPostDto);
    }

    public class TextPostService : ITextPostService
    {
        private readonly ITextPostRepository _textPostRepository;

        public TextPostService(ITextPostRepository textPostRepository)
        {
            _textPostRepository = textPostRepository ?? throw new ArgumentNullException(nameof(textPostRepository));
        }

        public async Task<IEnumerable<TextPostDto>> GetAllTextPosts()
        {
            var textPosts = await _textPostRepository.GetAllTextPosts();
            return textPosts.Select(textPost => new TextPostDto
            {
                PostID = textPost.PostID,
                TextBody = textPost.TextBody
            });
        }

        public async Task InsertTextPost(PostDto postDto, TextPostDto textPostDto)
        {
            var post = new Post
            {
                MediaType = postDto.MediaType,
                CreatedAt = postDto.CreatedAt,
                ExpiresAt = postDto.ExpiresAt,
                Name = postDto.Name,
                Caption = postDto.Caption
            };

            var textPost = new TextPost
            {
                TextBody = textPostDto.TextBody
            };

            await _textPostRepository.InsertTextPost(post, textPost);
        }
    }
}
