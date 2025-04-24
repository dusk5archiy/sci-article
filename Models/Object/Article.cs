namespace SciArticle.Models.Object;

class ArticleStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public DateOnly TimeStamp { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string Abstract { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}