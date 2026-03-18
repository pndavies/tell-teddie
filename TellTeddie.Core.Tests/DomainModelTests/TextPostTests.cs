using System.Text.Json;
using TellTeddie.Core.DomainModels;
using Xunit;

namespace TellTeddie.Core.Tests.DomainModelTests
{
    public class TextPostTests
    {
        [Fact]
        public void PropertyAssignment_AllowsSettingAndGettingValues()
        {
            // Arrange / Act
            var post = new TextPost
            {
                PostID = 123,
                TextBody = "Hello TellTeddie"
            };

            // Assert
            Assert.Equal(123, post.PostID);
            Assert.Equal("Hello TellTeddie", post.TextBody);
        }

        [Fact]
        public void JsonSerialization_RoundTrips_WithTextBody()
        {
            var original = new TextPost
            {
                PostID = 8,
                TextBody = "Sample text"
            };

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<TextPost>(json, options);

            Assert.NotNull(deserialized);
            Assert.Equal(original.PostID, deserialized!.PostID);
            Assert.Equal(original.TextBody, deserialized.TextBody);
        }

        [Fact]
        public void JsonSerialization_RoundTrips_WithNullTextBody()
        {
            var original = new TextPost
            {
                PostID = 9,
                TextBody = null
            };

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<TextPost>(json, options);

            Assert.NotNull(deserialized);
            Assert.Equal(original.PostID, deserialized!.PostID);
            Assert.Null(deserialized.TextBody);
        }
    }
}