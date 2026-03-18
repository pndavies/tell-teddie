using System.Text.Json;
using TellTeddie.Core.DomainModels;

namespace TellTeddie.Core.Tests.DomainModelTests
{
    public class PostTests
    {
        [Fact]
        public void PropertyAssignment_AllowsSettingAndGettingValues()
        {
            // Arrange
            var created = DateTime.UtcNow;
            var expires = created.AddDays(7);

            // Act
            var post = new Post
            {
                PostID = 42,
                MediaType = "Audio",
                CreatedAt = created,
                ExpiresAt = expires,
                TextPost = new TextPost
                {
                    PostID = 42,
                    TextBody = "Some text body for bear"
                },
                AudioPost = new AudioPost
                {
                    PostID = 42,
                    AudioPostUrl = "https://somebearUrl.com-"
                }
            };

            // Assert
            Assert.Equal(42, post.PostID);
            Assert.Equal("Audio", post.MediaType);
            Assert.Equal(created, post.CreatedAt);
            Assert.Equal(expires, post.ExpiresAt);
            Assert.Equal(42, post.TextPost.PostID);
            Assert.Equal("Some text body for bear", post.TextPost.TextBody);
            Assert.Equal(42, post.AudioPost.PostID);
            Assert.Equal("https://somebearUrl.com-", post.AudioPost.AudioPostUrl);
        }

        [Fact]
        public void JsonSerialization_RoundTripsPreservingAllProperties()
        {
            // Arrange
            var created = DateTime.UtcNow;
            var expires = created.AddHours(48);

            var original = new Post
            {
                PostID = 7,
                MediaType = "Text",
                CreatedAt = created,
                ExpiresAt = expires,
                TextPost = new TextPost
                {
                    PostID = 42,
                    TextBody = "Some text body for bear"
                },
                AudioPost = new AudioPost
                {
                    PostID = 42,
                    AudioPostUrl = "https://somebearUrl.com-"
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<Post>(json, options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.PostID, deserialized!.PostID);
            Assert.Equal(original.MediaType, deserialized.MediaType);
            Assert.Equal(original.CreatedAt, deserialized.CreatedAt);
            Assert.Equal(original.ExpiresAt, deserialized.ExpiresAt);
            Assert.Equal(original.TextPost.PostID, deserialized.TextPost!.PostID);
            Assert.Equal(original.TextPost.TextBody, deserialized.TextPost.TextBody);
            Assert.Equal(original.AudioPost.PostID, deserialized.AudioPost!.PostID);
        }
    }
}
