using SciArticle.Models.Front.User;

namespace SciArticle.Models.Front.Admin;

public class AuthorStatisticsViewModel
{
    public string AdminName { get; set; } = string.Empty;
    public List<AuthorArticleCountViewModel> AuthorArticleCounts { get; set; } = new List<AuthorArticleCountViewModel>();
    public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    
    // Statistics by Status
    public int TotalArticles { get; set; }
    public int ApprovedArticles { get; set; }
    public int PendingArticles { get; set; }
    public int RejectedArticles { get; set; }
    
    // Statistics by Topic
    public List<TopicArticleCountViewModel> TopicArticleCounts { get; set; } = new List<TopicArticleCountViewModel>();
    public PaginationInfo TopicPagination { get; set; } = new PaginationInfo();
    public string CurrentView { get; set; } = "authors"; // Can be "authors" or "topics"
    
    // Sorting properties for authors
    public string SortBy { get; set; } = "ArticleCount";
    public string SortOrder { get; set; } = "desc";
    
    // Sorting properties for topics
    public string TopicSortBy { get; set; } = "ArticleCount";
    public string TopicSortOrder { get; set; } = "desc";
}

public class AuthorArticleCountViewModel
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int ArticleCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public int RejectedCount { get; set; }
}

public class TopicArticleCountViewModel
{
    public string TopicName { get; set; } = string.Empty;
    public int ArticleCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public int RejectedCount { get; set; }
}