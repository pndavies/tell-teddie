using Dapper;
using Microsoft.Data.SqlClient;
using TellTeddie.Core.DomainModels;
using System.Data;
using System.Net.Http.Headers;
using static TellTeddie.Core.Enums.TellTeddieEnums;

namespace TellTeddie.Infrastructure.Repositories;

public interface ITextPostRepository
{
    Task<IEnumerable<TextPost>> GetAllTextPosts();
    Task InsertTextPost(Post post, TextPost textPost);
    Task DeleteExpiredTextPosts();
}

public class TextPostRepository : ITextPostRepository
{
    private readonly string _tellTeddieDbConnectionString;
    private IDbConnection CreateDBConnection() => new SqlConnection(_tellTeddieDbConnectionString);

    public TextPostRepository(string tellTeddieDbConnectionString)
    {
        _tellTeddieDbConnectionString = tellTeddieDbConnectionString ?? throw new ArgumentNullException(nameof(tellTeddieDbConnectionString));
    }

    public async Task<IEnumerable<TextPost>> GetAllTextPosts()
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();

        var sql = @"SELECT p.PostID, tp.TextBody FROM TextPost tp 
                    JOIN Post p ON p.PostID = tp.PostID
                    WHERE p.ExpiresAt > @DateTimeNow
                    ORDER BY p.CreatedAt DESC;";

        return await dBConnection.QueryAsync<TextPost>(sql, new { DateTimeNow = DateTime.UtcNow });
    }

    public async Task InsertTextPost(Post post, TextPost textPost)
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();
        using var transaction =  dBConnection.BeginTransaction();

        try
        {
            var postInsertSql = @"INSERT INTO Post (MediaType, CreatedAt, ExpiresAt, Name, Caption)
                                  VALUES (@MediaType, @CreatedAt, @ExpiresAt, @Name, @Caption); 
                                  SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertAndGetPostId = await dBConnection.QuerySingleAsync<int>(postInsertSql, new
            {
                MediaType = post.MediaType,
                CreatedAt = post.CreatedAt,
                ExpiresAt = post.ExpiresAt,
                Name = post.Name,
                Caption = post.Caption
            },
            transaction);


            var textPostInsertSql = @"INSERT INTO TextPost (PostID, TextBody) 
                                      VALUES (@PostID, @TextBody);";

            await dBConnection.ExecuteAsync(textPostInsertSql, new { PostID = insertAndGetPostId, TextBody = textPost.TextBody }, transaction);

            transaction.Commit();
        }
        catch (Exception ex) 
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteExpiredTextPosts()
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();
        using var transaction = dBConnection.BeginTransaction();

        try
        {
            var sql = @"
                DELETE tp
                FROM TextPost tp
                INNER JOIN Post p ON tp.PostID = p.PostID
                WHERE p.ExpiresAt < GETUTCDATE();
                
                DELETE FROM Post WHERE ExpiresAt < GETUTCDATE() AND MediaType = @MediaType;";

            await dBConnection.ExecuteAsync(sql, new { MediaType = PostType.TEXT.ToString()  }, transaction: transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
}
