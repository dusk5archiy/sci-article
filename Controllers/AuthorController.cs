using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SciArticle.Models;
using SciArticle.Models.Front.User;
using SciArticle.Models.Back;
using SciArticle.Models.Object;

namespace SciArticle.Controllers;

[Authorize(Roles = UserRole.Author)]
public class AuthorController : Controller
{
    private readonly ILogger<AuthorController> _logger;
    private readonly MainContext _context;
    private const int ItemsPerPage = 10;

    public AuthorController(ILogger<AuthorController> logger, MainContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Helper method to get the current authenticated user
    private User GetCurrentUser()
    {
        string username = User.Identity?.Name ?? string.Empty;
        return UserQuery.GetUserByUsername(username) ?? new();
    }

    public IActionResult Dashboard(int page = 1)
    {
        var user = GetCurrentUser();
        
        // Get the total count of articles for this author
        int totalItems = ArticleQuery.GetArticleCountByAuthor(user.Id);
        
        // Calculate pagination values
        page = Math.Max(1, page); // Ensure page is at least 1
        
        // Get the articles for this author with pagination
        var articles = ArticleQuery.GetArticlesByAuthor(user.Id, page, ItemsPerPage);
        
        // Map to view models
        var articleViewModels = articles.Select(a => new AuthorArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            TimeStamp = a.TimeStamp,
            Topic = a.Topic,
            Status = a.Status
        }).ToList();

        // Create the view model
        var viewModel = new AuthorDashboardViewModel
        {
            AuthorName = user.Name,
            Articles = articleViewModels,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = ItemsPerPage,
                TotalItems = totalItems
            }
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ArticleEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ArticleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = GetCurrentUser();

        // Create the article and pass the author's user ID
        Article article = ArticleQuery.CreateArticle(model, user.Id);

        _logger.LogInformation("Article {Id} created by {Username} (UserID: {UserId}) at {Time}", 
            article.Id, user.Username, user.Id, DateTime.Now);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = GetCurrentUser();

        // Get the article
        var article = ArticleQuery.GetArticleById(id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }

        // Check if article can be edited
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bài báo này không thể được chỉnh sửa vì đã được xem xét.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Create the view model
        var viewModel = new ArticleEditViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Abstract = article.Abstract,
            Content = article.Content,
            Topic = article.Topic
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ArticleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = GetCurrentUser();

        // Get the article
        var article = ArticleQuery.GetArticleById(model.Id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }

        // Check if article can be edited
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bài báo này không thể được chỉnh sửa vì đã được xem xét.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Update the article using ArticleQuery
        ArticleQuery.EditArticle(model);

        _logger.LogInformation("Article {Id} updated by {Username} at {Time}", 
            model.Id, user.Username, DateTime.Now);

        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult Details(int id)
    {
        var user = GetCurrentUser();

        // Get the article
        var article = ArticleQuery.GetArticleById(id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }

        // Create the view model
        var viewModel = new ArticleDetailViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Abstract = article.Abstract,
            Content = article.Content,
            Topic = article.Topic,
            TimeStamp = article.TimeStamp,
            Status = article.Status,
            CanEdit = article.Status == ArticleStatus.Pending
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id, int page = 1)
    {
        var user = GetCurrentUser();

        // Get the article
        var article = ArticleQuery.GetArticleById(id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }

        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bài báo này không thể bị hủy vì đã được xem xét.";
            return RedirectToAction(nameof(Dashboard), new { page });
        }

        ArticleQuery.CancelArticle(article.Id);
        _logger.LogInformation("Article {Id} canceled by {Username} at {Time}", 
            article.Id, user.Username, DateTime.Now);
        
        TempData["SuccessMessage"] = "Bài báo của bạn đã được hủy thành công.";
        
        // Get total number of articles after deletion
        int totalArticles = ArticleQuery.GetArticleCountByAuthor(user.Id);
        int totalPages = (int)Math.Ceiling((double)totalArticles / ItemsPerPage);
        
        // If current page exceeds total pages after deletion, go to last available page
        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
        }
        // If no articles left, go to page 1
        else if (totalPages == 0)
        {
            page = 1;
        }
        
        return RedirectToAction(nameof(Dashboard), new { page });
    }
}