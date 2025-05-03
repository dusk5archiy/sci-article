namespace SciArticle.Models.Front.User;

public class ArticleDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateOnly TimeStamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
}