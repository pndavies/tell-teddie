using Dapper;
using Microsoft.Data.SqlClient;
using TellTeddie.Core.DomainModels;
using System.Data;
using static TellTeddie.Core.Enums.TellTeddieEnums;


namespace TellTeddie.Infrastructure.Repositories;

public interface IAudioPostRepository
{
    Task<IEnumerable<AudioPost>> GetAllAudioPosts();
    Task InsertAudioPost(Post post, AudioPost audioPost);
    Task<IEnumerable<string>> GetExpiredAudioUrls();
    Task DeleteExpiredAudioPosts();
}

public class AudioPostRepository : IAudioPostRepository
{
    private readonly string _tellTeddieDbConnectionString;
    public AudioPostRepository(string tellTeddieDbConnectionString)
    {
        _tellTeddieDbConnectionString = tellTeddieDbConnectionString ?? throw new ArgumentNullException(nameof(tellTeddieDbConnectionString));
    }

    private IDbConnection CreateDBConnection() => new SqlConnection(_tellTeddieDbConnectionString);

    public async Task<IEnumerable<AudioPost>> GetAllAudioPosts()
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();

        var sql = @"SELECT p.PostID, ap.AudioPostUrl FROM AudioPost ap 
                    JOIN Post p ON p.PostID = ap.PostID
                    WHERE p.ExpiresAt > @DateTimeNow
                    ORDER BY p.CreatedAt DESC;";

        return await dBConnection.QueryAsync<AudioPost>(sql, new { DateTimeNow = DateTime.UtcNow });
    }

    public async Task InsertAudioPost(Post post, AudioPost audioPost)
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();
        using var transaction = dBConnection.BeginTransaction();

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


            var audioPostInsertSql = @"INSERT INTO AudioPost (PostID, AudioPostUrl) 
                                       VALUES (@PostID, @AudioPostUrl);";

            await dBConnection.ExecuteAsync(audioPostInsertSql, new { PostID = insertAndGetPostId, AudioPostUrl = audioPost.AudioPostUrl }, transaction);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetExpiredAudioUrls()
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();

        var sql = @"
        SELECT ap.AudioPostUrl
        FROM AudioPost ap
        INNER JOIN Post p ON ap.PostID = p.PostID
        WHERE p.ExpiresAt < GETUTCDATE();";
        var expiredAudioUrls = await dBConnection.QueryAsync<string>(sql);

        return expiredAudioUrls;
    }

    public async Task DeleteExpiredAudioPosts()
    {
        using var dBConnection = CreateDBConnection();
        dBConnection.Open();
        using var transaction = dBConnection.BeginTransaction();

        try
        {
            var sql = @"
            DELETE ap
            FROM AudioPost ap
            INNER JOIN Post p ON ap.PostID = p.PostID
            WHERE p.ExpiresAt < GETUTCDATE();

            DELETE FROM Post WHERE ExpiresAt < GETUTCDATE() AND MediaType = @MediaType;";

            await dBConnection.ExecuteAsync(sql, new { MediaType = PostType.AUDIO.ToString() }, transaction: transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
}
