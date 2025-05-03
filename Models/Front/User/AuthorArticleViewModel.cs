using SciArticle.Models.Object;

namespace SciArticle.Models.Front.User;

public class AuthorArticleViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly TimeStamp { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // This property is used to determine if the edit button should be shown
    public bool CanEdit => Status == ArticleStatus.Pending;
}