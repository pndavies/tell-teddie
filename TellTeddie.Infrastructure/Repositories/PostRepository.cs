using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using TellTeddie.Core.DomainModels;

namespace TellTeddie.Infrastructure.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllPosts();
    }
    public class PostRepository : IPostRepository
    {
        private readonly string _tellTeddieDbConnectionString;
        private IDbConnection CreateDBConnection() => new SqlConnection(_tellTeddieDbConnectionString);

        public PostRepository(string tellTeddieDbConnectionString)
        {
            _tellTeddieDbConnectionString = tellTeddieDbConnectionString ?? throw new ArgumentNullException(nameof(tellTeddieDbConnectionString));
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            using var dBConnection = CreateDBConnection();
            dBConnection.Open();

            var sql = @"SELECT 
                        p.PostId,
                        p.MediaType,
                        p.CreatedAt,
                        p.ExpiresAt,
                        p.Name, 
                        p.Caption
                    FROM Post p
                    WHERE p.ExpiresAt > @DateTimeNow
                    ORDER BY p.CreatedAt DESC";
            
            var posts = await dBConnection.QueryAsync<Post>(sql, new { DateTimeNow = DateTime.UtcNow });

            return posts;
        }

    }
}
