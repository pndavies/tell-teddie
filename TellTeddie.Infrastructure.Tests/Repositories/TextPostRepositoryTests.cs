using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using TellTeddie.Infrastructure.Repositories;
using TellTeddie.Core.DomainModels;
using System.IO;

namespace TellTeddie.Infrastructure.Tests.Repositories
{
    [Trait("Category", "Integration")]
    public class TextPostRepositoryTests
    {
        private readonly string? _testConnectionString;

        public TextPostRepositoryTests()
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
            Assert.Throws<ArgumentNullException>(() => new TextPostRepository(null!));
        }

        [Fact]
        public async Task GetAllTextPosts_Integration_ReturnsInsertedRow()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            // Arrange
            var uniqueText = "integration-" + Guid.NewGuid();
            var created = DateTime.UtcNow;
            var expires = created.AddHours(1);

            int insertedPostId;
            using (var conn = new SqlConnection(_testConnectionString))
            {
                await conn.OpenAsync();
                var postSql = @"INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                                VALUES (@MediaType, @CreatedAt, @ExpiresAt, 'PetTest', 'Caption of Pet');
                                SELECT CAST(SCOPE_IDENTITY() as int);";
                insertedPostId = await conn.QuerySingleAsync<int>(postSql, new { MediaType = "Text", CreatedAt = created, ExpiresAt = expires });

                var textSql = @"INSERT INTO TextPost (PostID, TextBody) VALUES (@PostID, @TextBody)";
                await conn.ExecuteAsync(textSql, new { PostID = insertedPostId, TextBody = uniqueText });
            }

            var repo = new TextPostRepository(_testConnectionString);

            try
            {
                // Act
                var results = await repo.GetAllTextPosts();

                // Assert
                Assert.Contains(results, t => t.TextBody == uniqueText);
            }
            finally
            {
                // Cleanup
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                await conn.ExecuteAsync("DELETE FROM TextPost WHERE PostID = @Id", new { Id = insertedPostId });
                await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = insertedPostId });
            }
        }

        [Fact]
        public async Task InsertTextPost_Integration_InsertsBothTablesAndReturnsScopeId()
        {
            CheckConnectionString();

            await EnsureSchema(_testConnectionString);

            var post = new Post
            {
                MediaType = "Text",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Name = "A Pet",
                Caption = "This is a caption for a pet."
            };

            var textPost = new TextPost
            {
                TextBody = "insert-test-" + Guid.NewGuid()
            };

            var repo = new TextPostRepository(_testConnectionString);

            int createdId = -1;
            try
            {
                // Act
                await repo.InsertTextPost(post, textPost);

                // Query by unique TextBody to verify insert
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                var row = await conn.QuerySingleOrDefaultAsync<(int PostID, string TextBody)>(
                    "SELECT tp.PostID, tp.TextBody FROM TextPost tp WHERE TextBody = @TextBody",
                    new { TextBody = textPost.TextBody });

                Assert.NotEqual(default((int, string)), row);
                Assert.Equal(textPost.TextBody, row.TextBody);
                createdId = row.PostID;
            }
            finally
            {
                // Cleanup
                if (createdId > 0)
                {
                    using var conn = new SqlConnection(_testConnectionString);
                    await conn.OpenAsync();
                    await conn.ExecuteAsync("DELETE FROM TextPost WHERE PostID = @Id", new { Id = createdId });
                    await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = createdId });
                }
            }
        }

        [Fact]
        public async Task DeleteExpiredTextPosts_Integration_DeletesExpiredRows()
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
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt)
                    VALUES ('Text', @CreatedAt, @ExpiresAtExpired); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow.AddDays(-10), ExpiresAtExpired = DateTime.UtcNow.AddDays(-1) });

                await conn.ExecuteAsync("INSERT INTO TextPost (PostID, TextBody) VALUES (@Id, @Body)", new { Id = expiredId, Body = "expired" });

                // Insert valid post
                validId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO Post (MediaType, CreatedAt, ExpiresAt)
                    VALUES ('Text', @CreatedAt, @ExpiresAtValid); SELECT CAST(SCOPE_IDENTITY() as int);",
                    new { CreatedAt = DateTime.UtcNow, ExpiresAtValid = DateTime.UtcNow.AddDays(7) });

                await conn.ExecuteAsync("INSERT INTO TextPost (PostID, TextBody) VALUES (@Id, @Body)", new { Id = validId, Body = "valid" });
            }

            var repo = new TextPostRepository(_testConnectionString);

            try
            {
                // Act
                await repo.DeleteExpiredTextPosts();

                // Assert: expired should be gone, valid should remain
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                var expiredExists = await conn.QuerySingleOrDefaultAsync<int?>("SELECT PostID FROM TextPost WHERE PostID = @Id", new { Id = expiredId });
                var validExists = await conn.QuerySingleOrDefaultAsync<int?>("SELECT PostID FROM TextPost WHERE PostID = @Id", new { Id = validId });

                Assert.Null(expiredExists);
                Assert.NotNull(validExists);
            }
            finally
            {
                // Cleanup remaining rows
                using var conn = new SqlConnection(_testConnectionString);
                await conn.OpenAsync();
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM TextPost WHERE PostID = @Id", new { Id = expiredId });
                if (expiredId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = expiredId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM TextPost WHERE PostID = @Id", new { Id = validId });
                if (validId > 0) await conn.ExecuteAsync("DELETE FROM Post WHERE PostID = @Id", new { Id = validId });
            }
        }

        private static async Task EnsureSchema(string connectionString)
        {
            // Create simple Post and TextPost tables if they don't exist
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
            var createTextPostSql = @"
                IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TextPost]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[TextPost](
                        [PostID] INT NOT NULL PRIMARY KEY,
                        [TextBody] NVARCHAR(MAX) NULL,
                        CONSTRAINT FK_TextPost_Post FOREIGN KEY (PostID) REFERENCES Post(PostID) ON DELETE CASCADE
                    );
                END";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(createPostSql);
            await conn.ExecuteAsync(createTextPostSql);
        }
    }
}