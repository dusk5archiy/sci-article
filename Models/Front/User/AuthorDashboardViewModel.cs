using System.Collections.Generic;

namespace SciArticle.Models.Front.User;

public class PaginationInfo
{
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
}

public class AuthorDashboardViewModel
{
    public string AuthorName { get; set; } = string.Empty;
    public List<AuthorArticleViewModel> Articles { get; set; } = new List<AuthorArticleViewModel>();
    public PaginationInfo Pagination { get; set; } = new PaginationInfo();
}