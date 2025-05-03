using SciArticle.Models.Object;

namespace SciArticle.Models.Front.Admin;

public class AdminArticleViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateOnly TimeStamp { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsPending => Status == ArticleStatus.Pending;
}