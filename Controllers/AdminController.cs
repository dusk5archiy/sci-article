using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models;
using SciArticle.Models.Back;
using SciArticle.Models.Front.Admin;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;

namespace SciArticle.Controllers;

[Authorize(Roles = UserRole.Admin)]
public class AdminController : Controller
{
    private const int ItemsPerPage = 10;

    public AdminController()
    {
    }

    private User? GetCurrentAdmin()
    {
        string username = User.Identity?.Name ?? string.Empty;
        return UserQuery.GetUserByUsername(username);
    }

    public IActionResult Dashboard(int page = 1, string statusFilter = "All")
    {
        var admin = GetCurrentAdmin();

        int totalItems = ArticleQuery.GetArticleCountByStatus(statusFilter);

        page = Math.Max(1, page);
        var articles = ArticleQuery.GetArticlesByStatus(statusFilter, page, ItemsPerPage);

        var authorIds = articles.Select(a => a.AuthorId).Distinct().ToList();
        var authors = new Dictionary<int, string>();

        foreach (var authorId in authorIds)
        {
            var author = UserQuery.GetUserById(authorId);
            if (author != null)
            {
                authors[authorId] = author.Name;
            }
        }

        var articleViewModels = articles
            .Select(a => new AdminArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                AuthorId = a.AuthorId,
                AuthorName = authors.ContainsKey(a.AuthorId) ? authors[a.AuthorId] : "Unknown",
                TimeStamp = a.TimeStamp,
                Topic = a.Topic,
                Status = a.Status,
            })
            .ToList();

        var viewModel = new AdminDashboardViewModel
        {
            AdminName = admin?.Name ?? "Admin",
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
        var admin = GetCurrentAdmin();
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
        var admin = GetCurrentAdmin();

        // Get the article to verify it exists
        var article = ArticleQuery.GetArticleById(id);
        if (article == null)
        {
            return NotFound();
        }

        // Use ArticleQuery to update the article status
        ArticleQuery.RejectArticle(id);
        TempData["SuccessMessage"] = "Bài báo đã bị từ chối.";
        return RedirectToAction(nameof(Dashboard), new { page, statusFilter });
    }

    public IActionResult Details(int id, int page = 1, string statusFilter = "All")
    {
        var admin = GetCurrentAdmin();

        // Get the article
        var article = ArticleQuery.GetArticleById(id);
        if (article == null)
        {
            return NotFound();
        }

        // Get the author of the article
        var author = UserQuery.GetUserById(article.AuthorId);
        string authorName = author?.Name ?? "Unknown";

        // Create the view model
        var viewModel = new AdminArticleDetailsViewModel
        {
            Id = article.Id,
            Title = article.Title,
            AuthorId = article.AuthorId,
            AuthorName = authorName,
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
        var admin = GetCurrentAdmin();

        // Calculate overall article statistics using our query methods
        int totalArticles = ArticleQuery.GetTotalArticleCount();
        int approvedArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Approved);
        int pendingArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Pending);
        int rejectedArticles = ArticleQuery.GetArticleCountByStatusType(ArticleStatus.Rejected);

        // Initialize view model
        var viewModel = new AuthorStatisticsViewModel
        {
            AdminName = admin?.Name ?? "Admin",
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

        // Get author statistics with pagination if viewing authors tab
        if (view == "authors" || view == "both")
        {
            // Ensure page is at least 1
            page = Math.Max(1, page);

            // Get total count of authors with articles
            int totalAuthors = ArticleQuery.GetAuthorWithArticlesCount();

            // Get author statistics with pagination
            var authorStats = ArticleQuery.GetAuthorArticleStatistics(page, ItemsPerPage, sortBy, sortOrder);

            // Set values in view model
            viewModel.AuthorArticleCounts = authorStats;
            viewModel.Pagination = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalAuthors
            };
        }

        // Get topic statistics with pagination if viewing topics tab
        if (view == "topics" || view == "both")
        {
            // Ensure page is at least 1
            topicPage = Math.Max(1, topicPage);

            // Get total count of topics
            int totalTopics = ArticleQuery.GetTopicCount();

            // Get topic statistics with pagination
            var topicStats = ArticleQuery.GetTopicArticleStatistics(topicPage, ItemsPerPage, topicSortBy, topicSortOrder);

            // Set values in view model
            viewModel.TopicArticleCounts = topicStats;
            viewModel.TopicPagination = new PaginationInfo
            {
                CurrentPage = topicPage,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalTopics
            };
        }

        return View(viewModel);
    }
}

