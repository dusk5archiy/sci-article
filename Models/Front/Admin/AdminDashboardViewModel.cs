using SciArticle.Models.Front.User;

namespace SciArticle.Models.Front.Admin;

public class AdminDashboardViewModel
{
    public string AdminName { get; set; } = string.Empty;
    public List<AdminArticleViewModel> Articles { get; set; } = new List<AdminArticleViewModel>();
    public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    public string StatusFilter { get; set; } = "All"; // All, Pending, Approved, Rejected
}