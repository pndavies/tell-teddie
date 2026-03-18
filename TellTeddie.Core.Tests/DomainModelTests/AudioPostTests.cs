using System.Text.Json;
using TellTeddie.Core.DomainModels;


namespace TellTeddie.Core.Tests.DomainModelTests
{
    public class AudioPostTests
    {
        [Fact]
        public void PropertyAssignment_AllowsSettingAndGettingValues()
        {
            // Arrange / Act
            var post = new AudioPost
            {
                PostID = 99,
                AudioPostUrl = "https://example.com/audio/99.webm"
            };

            // Assert
            Assert.Equal(99, post.PostID);
            Assert.Equal("https://example.com/audio/99.webm", post.AudioPostUrl);
        }

        [Fact]
        public void JsonSerialization_RoundTrips_WithUrl()
        {
            var original = new AudioPost
            {
                PostID = 5,
                AudioPostUrl = "https://cdn.example.com/5.webm"
            };

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<AudioPost>(json, options);

            Assert.NotNull(deserialized);
            Assert.Equal(original.PostID, deserialized!.PostID);
            Assert.Equal(original.AudioPostUrl, deserialized.AudioPostUrl);
        }

        [Fact]
        public void JsonSerialization_RoundTrips_WithNullUrl()
        {
            var original = new AudioPost
            {
                PostID = 7,
                AudioPostUrl = null
            };

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<AudioPost>(json, options);

            Assert.NotNull(deserialized);
            Assert.Equal(original.PostID, deserialized!.PostID);
            Assert.Null(deserialized.AudioPostUrl);
        }
    }
}