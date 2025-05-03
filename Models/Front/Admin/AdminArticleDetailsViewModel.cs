namespace SciArticle.Models.Front.Admin;

public class AdminArticleDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateOnly TimeStamp { get; set; }
    public string Abstract { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // For navigation after actions
    public int Page { get; set; } = 1;
    public string StatusFilter { get; set; } = "All";
}