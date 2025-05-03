using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models;
using SciArticle.Models.Front.Admin;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;
using SciArticle.Models.Back;
using System.Linq;

namespace SciArticle.Controllers;

[Authorize(Roles = UserRole.Admin)]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly MainContext _context;
    private const int ItemsPerPage = 10;

    public AdminController(ILogger<AdminController> logger, MainContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Dashboard(int page = 1, string statusFilter = "All")
    {
        // Get the current admin's username
        string username = User.Identity?.Name ?? string.Empty;
        
        // Find the admin by username
        var admin = _context.User.FirstOrDefault(u => u.Username == username && u.Role == UserRole.Admin);
        if (admin == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Query for articles with optional status filter
        var articlesQuery = _context.Article.AsQueryable();
        
        // Apply status filter if not "All"
        if (statusFilter != "All")
        {
            articlesQuery = articlesQuery.Where(a => a.Status == statusFilter);
        }
        
        // Get the total count of articles with the filter applied
        int totalItems = articlesQuery.Count();
        
        // Calculate pagination values
        page = Math.Max(1, page); // Ensure page is at least 1
        int skip = (page - 1) * ItemsPerPage;
        
        // Get the articles with pagination
        var articles = articlesQuery
            .OrderByDescending(a => a.Id)
            .Skip(skip)
            .Take(ItemsPerPage)
            .ToList();

        // Get author names for the articles
        var authorIds = articles.Select(a => a.AuthorId).Distinct().ToList();
        var authors = _context.User.Where(u => authorIds.Contains(u.Id)).ToDictionary(u => u.Id, u => u.Name);

        // Create view models for the articles
        var articleViewModels = articles.Select(a => new AdminArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            AuthorId = a.AuthorId,
            AuthorName = authors.ContainsKey(a.AuthorId) ? authors[a.AuthorId] : "Unknown",
            TimeStamp = a.TimeStamp,
            Topic = a.Topic,
            Status = a.Status
        }).ToList();

        // Create the view model
        var viewModel = new AdminDashboardViewModel
        {
            AdminName = admin.Name,
            Articles = articleViewModels,
            StatusFilter = statusFilter,
            Pagination = new Models.Front.User.PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalItems
            }
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApproveArticle(int id, int page = 1, string statusFilter = "All")
    {
        // Get the current admin's username
        string username = User.Identity?.Name ?? string.Empty;
        var admin = _context.User.FirstOrDefault(u => u.Username == username && u.Role == UserRole.Admin);
        if (admin == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article to verify it exists
        var article = _context.Article.FirstOrDefault(a => a.Id == id);
        if (article == null)
        {
            return NotFound();
        }

        // Use ArticleQuery to update the article status
        ArticleQuery.ApproveArticle(id);

        _logger.LogInformation("Article {Id} approved by admin {Username} at {Time}", 
            article.Id, admin.Username, DateTime.Now);
        
        TempData["SuccessMessage"] = "Bài báo đã được phê duyệt thành công.";
        return RedirectToAction(nameof(Dashboard), new { page, statusFilter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectArticle(int id, int page = 1, string statusFilter = "All")
    {
        // Get the current admin's username
        string username = User.Identity?.Name ?? string.Empty;
        var admin = _context.User.FirstOrDefault(u => u.Username == username && u.Role == UserRole.Admin);
        if (admin == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article to verify it exists
        var article = _context.Article.FirstOrDefault(a => a.Id == id);
        if (article == null)
        {
            return NotFound();
        }

        // Use ArticleQuery to update the article status
        ArticleQuery.RejectArticle(id);

        _logger.LogInformation("Article {Id} rejected by admin {Username} at {Time}", 
            article.Id, admin.Username, DateTime.Now);
        
        TempData["SuccessMessage"] = "Bài báo đã bị từ chối.";
        return RedirectToAction(nameof(Dashboard), new { page, statusFilter });
    }

    public IActionResult Details(int id, int page = 1, string statusFilter = "All")
    {
        // Get the current admin's username
        string username = User.Identity?.Name ?? string.Empty;
        var admin = _context.User.FirstOrDefault(u => u.Username == username && u.Role == UserRole.Admin);
        if (admin == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article
        var article = _context.Article.FirstOrDefault(a => a.Id == id);
        if (article == null)
        {
            return NotFound();
        }

        // Get the author of the article
        var author = _context.User.FirstOrDefault(u => u.Id == article.AuthorId);
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
            StatusFilter = statusFilter
        };

        return View(viewModel);
    }

    public IActionResult Statistics(int page = 1, int topicPage = 1, string view = "authors", 
        string sortBy = "ArticleCount", string sortOrder = "desc",
        string topicSortBy = "ArticleCount", string topicSortOrder = "desc")
    {
        // Get the current admin's username
        string username = User.Identity?.Name ?? string.Empty;
        var admin = _context.User.FirstOrDefault(u => u.Username == username && u.Role == UserRole.Admin);
        if (admin == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get all articles for counting
        var allArticles = _context.Article.ToList();
        
        // Calculate overall article statistics
        int totalArticles = allArticles.Count;
        int approvedArticles = allArticles.Count(a => a.Status == ArticleStatus.Approved);
        int pendingArticles = allArticles.Count(a => a.Status == ArticleStatus.Pending);
        int rejectedArticles = allArticles.Count(a => a.Status == ArticleStatus.Rejected);

        // Initialize view model
        var viewModel = new AuthorStatisticsViewModel
        {
            AdminName = admin.Name,
            TotalArticles = totalArticles,
            ApprovedArticles = approvedArticles,
            PendingArticles = pendingArticles,
            RejectedArticles = rejectedArticles,
            CurrentView = view,
            SortBy = sortBy,
            SortOrder = sortOrder,
            TopicSortBy = topicSortBy,
            TopicSortOrder = topicSortOrder
        };

        // Calculate authors statistics with pagination
        if (view == "authors" || view == "both")
        {
            var authors = _context.User
                .Where(u => u.Role == UserRole.Author)
                .Select(u => new { 
                    User = u,
                    ArticleCount = _context.Article.Count(a => a.AuthorId == u.Id),
                    ApprovedCount = _context.Article.Count(a => a.AuthorId == u.Id && a.Status == ArticleStatus.Approved),
                    PendingCount = _context.Article.Count(a => a.AuthorId == u.Id && a.Status == ArticleStatus.Pending),
                    RejectedCount = _context.Article.Count(a => a.AuthorId == u.Id && a.Status == ArticleStatus.Rejected)
                })
                .Where(a => a.ArticleCount > 0)
                .AsEnumerable();

            // Apply sorting
            authors = sortBy switch
            {
                "AuthorName" => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.User.Name) 
                    : authors.OrderByDescending(a => a.User.Name),
                "ArticleCount" => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.ArticleCount) 
                    : authors.OrderByDescending(a => a.ArticleCount),
                "ApprovedCount" => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.ApprovedCount) 
                    : authors.OrderByDescending(a => a.ApprovedCount),
                "PendingCount" => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.PendingCount) 
                    : authors.OrderByDescending(a => a.PendingCount),
                "RejectedCount" => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.RejectedCount) 
                    : authors.OrderByDescending(a => a.RejectedCount),
                _ => sortOrder == "asc" 
                    ? authors.OrderBy(a => a.ArticleCount) 
                    : authors.OrderByDescending(a => a.ArticleCount)
            };
                
            // Get total count of authors with articles for pagination
            int totalAuthors = authors.Count();
            
            // Calculate pagination values for authors
            page = Math.Max(1, page); // Ensure page is at least 1
            int skipAuthors = (page - 1) * ItemsPerPage;
            
            // Get the authors with pagination
            var paginatedAuthors = authors
                .Skip(skipAuthors)
                .Take(ItemsPerPage)
                .ToList();

            // Create view models for the authors with article counts
            var authorViewModels = paginatedAuthors.Select(a => new AuthorArticleCountViewModel
            {
                AuthorId = a.User.Id,
                AuthorName = a.User.Name,
                ArticleCount = a.ArticleCount,
                ApprovedCount = a.ApprovedCount,
                PendingCount = a.PendingCount,
                RejectedCount = a.RejectedCount
            }).ToList();

            viewModel.AuthorArticleCounts = authorViewModels;
            viewModel.Pagination = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalAuthors
            };
        }

        // Calculate topic statistics with pagination
        if (view == "topics" || view == "both")
        {
            // Group articles by topic and count by status
            var topics = allArticles
                .GroupBy(a => a.Topic)
                .Select(g => new {
                    TopicName = g.Key,
                    ArticleCount = g.Count(),
                    ApprovedCount = g.Count(a => a.Status == ArticleStatus.Approved),
                    PendingCount = g.Count(a => a.Status == ArticleStatus.Pending),
                    RejectedCount = g.Count(a => a.Status == ArticleStatus.Rejected)
                })
                .ToList();

            // Apply sorting for topics
            topics = topicSortBy switch
            {
                "TopicName" => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.TopicName).ToList() 
                    : topics.OrderByDescending(t => t.TopicName).ToList(),
                "ArticleCount" => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.ArticleCount).ToList() 
                    : topics.OrderByDescending(t => t.ArticleCount).ToList(),
                "ApprovedCount" => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.ApprovedCount).ToList() 
                    : topics.OrderByDescending(t => t.ApprovedCount).ToList(),
                "PendingCount" => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.PendingCount).ToList() 
                    : topics.OrderByDescending(t => t.PendingCount).ToList(),
                "RejectedCount" => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.RejectedCount).ToList() 
                    : topics.OrderByDescending(t => t.RejectedCount).ToList(),
                _ => topicSortOrder == "asc" 
                    ? topics.OrderBy(t => t.ArticleCount).ToList() 
                    : topics.OrderByDescending(t => t.ArticleCount).ToList()
            };

            // Get total count of topics for pagination
            int totalTopics = topics.Count;
            
            // Calculate pagination values for topics
            topicPage = Math.Max(1, topicPage); // Ensure page is at least 1
            int skipTopics = (topicPage - 1) * ItemsPerPage;
            
            // Get the topics with pagination
            var paginatedTopics = topics
                .Skip(skipTopics)
                .Take(ItemsPerPage)
                .ToList();

            // Create view models for the topics with article counts
            var topicViewModels = paginatedTopics.Select(t => new TopicArticleCountViewModel
            {
                TopicName = t.TopicName,
                ArticleCount = t.ArticleCount,
                ApprovedCount = t.ApprovedCount,
                PendingCount = t.PendingCount,
                RejectedCount = t.RejectedCount
            }).ToList();

            viewModel.TopicArticleCounts = topicViewModels;
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