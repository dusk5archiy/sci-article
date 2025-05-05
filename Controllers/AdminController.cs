using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models.Back;
using SciArticle.Models.Front.Admin;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;

namespace SciArticle.Controllers;

[Authorize(Roles = UserRole.Admin)]
public class AdminController : Controller
{
    private const int ItemsPerPage = 10;

    private User? GetCurrentUser()
    {
        string username = User.Identity?.Name ?? string.Empty;
        return UserQuery.GetUserByUsername(username);
    }

    public IActionResult Dashboard(int page = 1, string statusFilter = "All")
    {
        page = Math.Max(1, page);
        int totalItems = ArticleQuery.GetArticleCountByStatus(statusFilter);
        var articleViewModels = ArticleQuery.GetArticlesForAdminDashboard(
            statusFilter,
            page,
            ItemsPerPage
        );

        var viewModel = new AdminDashboardViewModel
        {
            AdminName = GetCurrentUser()?.Name ?? "Admin",
            Articles = articleViewModels,
            StatusFilter = statusFilter,
            Pagination = new()
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalItems,
            },
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApproveArticle(int id, int page = 1, string statusFilter = "All")
    {
        var article = ArticleQuery.GetArticleById(id);
        if (article == null)
        {
            return NotFound();
        }

        ArticleQuery.ApproveArticle(id);
        TempData["SuccessMessage"] = "Bài báo đã được phê duyệt thành công.";
        return RedirectToAction(nameof(Dashboard), new { page, statusFilter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectArticle(int id, int page = 1, string statusFilter = "All")
    {
        var article = ArticleQuery.GetArticleById(id);
        if (article == null)
        {
            return NotFound();
        }

        ArticleQuery.RejectArticle(id);
        TempData["SuccessMessage"] = "Bài báo đã bị từ chối.";
        return RedirectToAction(nameof(Dashboard), new { page, statusFilter });
    }

    public IActionResult Details(int id, int page = 1, string statusFilter = "All")
    {
        var article = ArticleQuery.GetArticleById(id);
        if (article == null)
        {
            return NotFound();
        }

        var author = UserQuery.GetUserById(article.AuthorId);
        var viewModel = new AdminArticleDetailsViewModel
        {
            Id = article.Id,
            Title = article.Title,
            AuthorId = article.AuthorId,
            AuthorName = author?.Name ?? "Unknown",
            TimeStamp = article.TimeStamp,
            Abstract = article.Abstract,
            Content = article.Content,
            Topic = article.Topic,
            Status = article.Status,
            Page = page,
            StatusFilter = statusFilter,
        };

        return View(viewModel);
    }

    public IActionResult Statistics(
        int page = 1,
        int topicPage = 1,
        string view = "authors",
        string sortBy = "ArticleCount",
        string sortOrder = "desc",
        string topicSortBy = "ArticleCount",
        string topicSortOrder = "desc"
    )
    {
        int totalArticles = ArticleQuery.GetTotalArticleCount();
        int approvedArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Approved);
        int pendingArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Pending);
        int rejectedArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Rejected);

        var viewModel = new AuthorStatisticsViewModel
        {
            AdminName = GetCurrentUser()?.Name ?? "Admin",
            TotalArticles = totalArticles,
            ApprovedArticles = approvedArticles,
            PendingArticles = pendingArticles,
            RejectedArticles = rejectedArticles,
            CurrentView = view,
            SortBy = sortBy,
            SortOrder = sortOrder,
            TopicSortBy = topicSortBy,
            TopicSortOrder = topicSortOrder,
        };

        if (view == "authors")
        {
            page = Math.Max(1, page);
            int totalAuthors = ArticleQuery.GetAuthorWithArticlesCount();
            var authorStats = ArticleQuery.GetAuthorArticleStatistics(
                page,
                ItemsPerPage,
                sortBy,
                sortOrder
            );
            viewModel.AuthorArticleCounts = authorStats;
            viewModel.Pagination = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalAuthors,
            };
        }

        if (view == "topics")
        {
            topicPage = Math.Max(1, topicPage);
            int totalTopics = ArticleQuery.GetTopicCount();
            var topicStats = ArticleQuery.GetTopicArticleStatistics(
                topicPage,
                ItemsPerPage,
                topicSortBy,
                topicSortOrder
            );
            viewModel.TopicArticleCounts = topicStats;
            viewModel.TopicPagination = new PaginationInfo
            {
                CurrentPage = topicPage,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalTopics,
            };
        }

        return View(viewModel);
    }
}
