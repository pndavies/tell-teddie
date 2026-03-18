namespace TellTeddie.Core.DomainModels;

public class Post
{
    public int PostID { get; set; }
    public required string MediaType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Name { get; set; }
    public string? Caption { get; set; }
    public TextPost? TextPost { get; set; }
    public AudioPost? AudioPost { get; set; }
}
