using TellTeddie.Contracts.DTOs;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;

namespace TellTeddie.Api.Services
{
    public interface IPostFeedService
    {
        Task<IEnumerable<PostDto>> GetAllPostsForFeed();
    }

    public class PostFeedService : IPostFeedService
    {
        private readonly IPostRepository _postRepository;
        private readonly ITextPostRepository _textPostRepository;
        private readonly IAudioPostRepository _audioPostRepository;

        public PostFeedService(IPostRepository postRepository, ITextPostRepository postTextRepository, IAudioPostRepository audioPostRepository)
        {
            _postRepository = postRepository;
            _textPostRepository = postTextRepository;
            _audioPostRepository = audioPostRepository;

        }

        public async Task<IEnumerable<PostDto>> GetAllPostsForFeed()
        {
            var posts = await _postRepository.GetAllPosts();
            var textPosts = await _textPostRepository.GetAllTextPosts();
            var audioPosts = await _audioPostRepository.GetAllAudioPosts();

            foreach (var post in posts)
            {
                post.TextPost = textPosts.FirstOrDefault(textPost => textPost.PostID == post.PostID);
                post.AudioPost = audioPosts.FirstOrDefault(audioPost => audioPost.PostID == post.PostID);
            }

            var allPosts = posts.Select(post => new PostDto
            {
                PostID = post.PostID,
                MediaType = post.MediaType,
                CreatedAt = post.CreatedAt,
                ExpiresAt = post.ExpiresAt,
                Name = post.Name,
                Caption = post.Caption,
                TextPost = post.TextPost != null
                    ? new TextPostDto
                    {
                        PostID = post.TextPost.PostID,
                        TextBody = post.TextPost.TextBody
                    }
                    : null,
                AudioPost = post.AudioPost != null
                    ? new AudioPostDto
                    {
                        PostID = post.AudioPost.PostID,
                        AudioPostUrl = post.AudioPost.AudioPostUrl
                    }
                    : null
            }).ToList();

            return allPosts;
        }
    }
    
}
