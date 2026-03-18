using System;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TellTeddie.Core.DomainModels;
using TellTeddie.Infrastructure.Repositories;
using Xunit;


namespace TellTeddie.Infrastructure.Tests.Repositories
{
    [Trait("Category", "Integration")]
    public class AudioPostRepositoryTests
    {
        private readonly string? _testConnectionString;

        public AudioPostRepositoryTests()
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
                .SetBasePath(projectRoot)
                .AddJsonFile(apiConfigPath, optional: true)
                .AddJsonFile(webConfigPath, optional: true)
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

        private void CheckConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_testConnectionString))
            {
                throw new InvalidOperationException("Integration test skipped. Configure appsettings.Development.json in Api or Web project.");
            }
        }

        [Fact]
        public void Constructor_NullConnectionString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioPostRepository(null!));
        }

        [Fact]
        public async Task GetAllAudioPosts_Integration_ReturnsInsertedRow()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            // Arrange
            var uniqueUrl = "integration-" + Guid.NewGuid() + ".webm";
            var created = DateTime.UtcNow;
            var expires = created.AddHours(1);

            int insertedPostId;
            using (var conn = new SqlConnection(_testConnectionString))
            {
                await conn.OpenAsync();
                var postSql = @"INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                                VALUES (@MediaType, @CreatedAt, @ExpiresAt, 'PetTest', 'Caption of Pet');
                                SELECT CAST(SCOPE_IDENTITY() as int);";
                insertedPostId = await conn.QuerySingleAsync<int>(postSql, new { MediaType = "Audio", CreatedAt = created, ExpiresAt = expires });

                var audioSql = @"INSERT INTO AudioPost (PostID, AudioPostUrl) VALUES (@PostID, @AudioPostUrl)";
                await conn.ExecuteAsync(audioSql, new { PostID = insertedPostId, AudioPostUrl = uniqueUrl });
            }

            var repo = new AudioPostRepository(_testConnectionString);

            try
            {
                // Act
                var results = await repo.GetAllAudioPosts();

                // Assert
                Assert.Contains(results, a => a.AudioPostUrl == uniqueUrl);
            }
            finally
            {
                // Cleanup
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = insertedPostId });
                await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = insertedPostId });
            }
        }

        [Fact]
        public async Task InsertAudioPost_Integration_InsertsBothTables()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            var post = new Post
            {
                MediaType = "Audio",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            var audioPost = new AudioPost
            {
                AudioPostUrl = "insert-audio-" + Guid.NewGuid() + ".webm"
            };

            var repo = new AudioPostRepository(_testConnectionString);

            int createdId = -1;
            try
            {
                // Act
                await repo.InsertAudioPost(post, audioPost);

                // Query by unique AudioPostUrl to verify insert
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                var row = await conn.QuerySingleOrDefaultAsync<(int PostID, string AudioPostUrl)>(
                    "SELECT ap.PostID, ap.AudioPostUrl FROM AudioPost ap WHERE AudioPostUrl = @AudioPostUrl",
                    new { AudioPostUrl = audioPost.AudioPostUrl });

                Assert.NotEqual(default((int, string)), row);
                Assert.Equal(audioPost.AudioPostUrl, row.AudioPostUrl);
                createdId = row.PostID;
            }
            finally
            {
                // Cleanup
                if (createdId > 0)
                {
                    using var conn = new SqlConnection(_testConnectionString);
                    await conn.OpenAsync();
                    await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = createdId });
                    await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = createdId });
                }
            }
        }

        [Fact]
        public async Task GetExpiredAudioUrls_Integration_ReturnsUrlsForExpiredPosts()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            int expiredId = -1;
            int validId = -1;

            using (var conn = new SqlConnection(_testConnectionString))
            {
                await conn.OpenAsync();

                // Insert expired post
                expiredId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                    VALUES ('Audio', @CreatedAt, @ExpiresAtExpired, 'PetTest', 'Caption of Pet'); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow.AddDays(-10), ExpiresAtExpired = DateTime.UtcNow.AddDays(-1) });

                await conn.ExecuteAsync("INSERT INTO AudioPost (PostID, AudioPostUrl) VALUES (@Id, @Url)", new { Id = expiredId, Url = "expired-url.webm" });

                // Insert valid post
                validId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                    VALUES ('Audio', @CreatedAt, @ExpiresAtValid, 'PetTest', 'Caption of Pet'); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow, ExpiresAtValid = DateTime.UtcNow.AddDays(7) });

                await conn.ExecuteAsync("INSERT INTO AudioPost (PostID, AudioPostUrl) VALUES (@Id, @Url)", new { Id = validId, Url = "valid-url.webm" });
            }

            var repo = new AudioPostRepository(_testConnectionString);

            try
            {
                // Act
                var expiredUrls = await repo.GetExpiredAudioUrls();

                // Assert
                Assert.Contains("expired-url.webm", expiredUrls);
                Assert.DoesNotContain("valid-url.webm", expiredUrls);
            }
            finally
            {
                // Cleanup remaining rows
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = expiredId });
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = expiredId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = validId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = validId });
            }
        }

        [Fact]
        public async Task DeleteExpiredAudioPosts_Integration_DeletesExpiredRows()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            int expiredId = -1;
            int validId = -1;

            using (var conn = new SqlConnection(_testConnectionString))
            {
                await conn.OpenAsync();

                // Insert expired post
                expiredId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                    VALUES ('Audio', @CreatedAt, @ExpiresAtExpired, 'PetTest', 'Caption of Pet'); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow.AddDays(-10), ExpiresAtExpired = DateTime.UtcNow.AddDays(-1) });

                await conn.ExecuteAsync("INSERT INTO AudioPost (PostID, AudioPostUrl) VALUES (@Id, @Url)", new { Id = expiredId, Url = "expired" });

                // Insert valid post
                validId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                    VALUES ('Audio', @CreatedAt, @ExpiresAtValid, 'PetTest', 'Caption of Pet'); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow, ExpiresAtValid = DateTime.UtcNow.AddDays(7) });

                await conn.ExecuteAsync("INSERT INTO AudioPost (PostID, AudioPostUrl) VALUES (@Id, @Url)", new { Id = validId, Url = "valid" });
            }

            var repo = new AudioPostRepository(_testConnectionString);

            try
            {
                // Act
                await repo.DeleteExpiredAudioPosts();

                // Assert: expired should be gone, valid should remain
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                var expiredExists = await conn.QuerySingleOrDefaultAsync<int?>("SELECT PostID FROM AudioPost WHERE PostID = @Id", new { Id = expiredId });
                var validExists = await conn.QuerySingleOrDefaultAsync<int?>("SELECT PostID FROM AudioPost WHERE PostID = @Id", new { Id = validId });

                Assert.Null(expiredExists);
                Assert.NotNull(validExists);
            }
            finally
            {
                // Cleanup remaining rows
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = expiredId });
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = expiredId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM AudioPost WHERE PostID = @Id", new { Id = validId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = validId });
            }
        }

        private static async Task EnsureSchema(string connectionString)
        {
            // Create simple Post and AudioPost tables if they don't exist
            var createPostSql = @"
                IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Post]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Post](
                        [PostID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [MediaType] NVARCHAR(50) NOT NULL,
                        [CreatedAt] DATETIME2 NOT NULL,
                        [ExpiresAt] DATETIME2 NOT NULL,
                        [Name] NVARCHAR(255) NULL,  
                        [Caption] NVARCHAR(MAX) NULL
                    );
                END";
            var createAudioPostSql = @"
                IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AudioPost]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[AudioPost](
                        [PostID] INT NOT NULL PRIMARY KEY,
                        [AudioPostUrl] NVARCHAR(MAX) NULL,
                        CONSTRAINT FK_AudioPost_Post FOREIGN KEY (PostID) REFERENCES Post(PostID) ON DELETE CASCADE
                    );
                END";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(createPostSql);
            await conn.ExecuteAsync(createAudioPostSql);
        }
    }
}