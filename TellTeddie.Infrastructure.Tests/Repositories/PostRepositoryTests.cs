using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TellTeddie.Infrastructure.Repositories;

namespace TellTeddie.Infrastructure.Tests.Repositories
{
    [Trait("Category", "Integration")]
    public class PostRepositoryTests
    {
        private readonly string? _testConnectionString;

        public PostRepositoryTests()
        {
            var projectRoot = GetProjectRoot();
            if (!Directory.Exists(projectRoot))
            {
                // fallback to test output folder which always exists at runtime
                projectRoot = AppContext.BaseDirectory;
            }

            var apiConfigPath = Path.Combine(projectRoot, "TellTeddie.Api", "appsettings.Development.json");
            var webConfigPath = Path.Combine(projectRoot, "TellTeddie.Web", "appsettings.Development.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(GetProjectRoot())
                .AddJsonFile("TellTeddie.Api/appsettings.Development.json", optional: true)
                .AddJsonFile("TellTeddie.Web/appsettings.Development.json", optional: true)
                .Build();

            _testConnectionString = config.GetConnectionString("tellTeddieLocalDbConnectionString");
        }

        private static string GetProjectRoot()
        {
            // Navigate up from test project bin/Debug/net8.0 to solution root
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "TellTeddie.sln")))
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? Directory.GetCurrentDirectory();
        }


        [Fact]
        public void Constructor_NullConnectionString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostRepository(null!));
        }

        [Fact]
        public async Task GetAllPosts_Integration_ReturnsInsertedRow()
        {
            // Arrange 
            var mediaType = "Text";
            var createdAt = DateTime.UtcNow;
            var expiresAt = createdAt.AddHours(1);
            int insertedPostId;
            using (var dBConnection = new SqlConnection(_testConnectionString))
            {
                dBConnection.Open();
                // Insert a test post
                var insertSql = @"INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                                  VALUES (@MediaType, @CreatedAt, @ExpiresAt, @Name, @Caption);
                                  SELECT CAST(SCOPE_IDENTITY() as int);";
                insertedPostId = await dBConnection.ExecuteScalarAsync<int>(insertSql, new
                {
                    MediaType = mediaType,
                    CreatedAt = createdAt,
                    ExpiresAt = expiresAt,
                    Name = "PetTest",
                    Caption = "Testing caption teds"
                });
            }

            var repo = new PostRepository(_testConnectionString!);

            try
            {
                // Act
                
                var posts = await repo.GetAllPosts();
                // Assert
                var insertedPost = posts.FirstOrDefault(p => p.PostID == insertedPostId);
                Assert.NotNull(insertedPost);
                Assert.Equal(mediaType, insertedPost!.MediaType);
                Assert.Equal(createdAt, insertedPost.CreatedAt, TimeSpan.FromSeconds(1)); // Allow slight time difference
                Assert.Equal(expiresAt, insertedPost.ExpiresAt, TimeSpan.FromSeconds(1));
            }
            finally
            {
                // Clean up - delete the test post
                using (var dBConnection = new SqlConnection(_testConnectionString))
                {
                    dBConnection.Open();
                    var deleteSql = "DELETE FROM Post WHERE PostID = @PostID";
                    await dBConnection.ExecuteAsync(deleteSql, new { PostID = insertedPostId });
                }
            }
        }
    }
}
