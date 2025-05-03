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

    public IActionResult Dashboard(int page = 1)
    {
        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        
        // Find the user by username
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the total count of articles for this author
        int totalItems = _context.Article.Count(a => a.AuthorId == user.Id);
        
        // Calculate pagination values
        page = Math.Max(1, page); // Ensure page is at least 1
        int skip = (page - 1) * ItemsPerPage;
        
        // Get the articles for this author with pagination
        // Changed from OrderByDescending(a => a.TimeStamp) to OrderByDescending(a => a.Id)
        var articles = _context.Article
            .Where(a => a.AuthorId == user.Id)
            .OrderByDescending(a => a.Id)
            .Skip(skip)
            .Take(ItemsPerPage)
            .Select(a => new AuthorArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                TimeStamp = a.TimeStamp,
                Topic = a.Topic,
                Status = a.Status
            })
            .ToList();

        // Create the view model
        var viewModel = new AuthorDashboardViewModel
        {
            AuthorName = user.Name,
            Articles = articles,
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

        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Create the article and pass the author's user ID
        Article article = ArticleQuery.CreateArticle(model, user.Id);

        _logger.LogInformation("Article {Id} created by {Username} (UserID: {UserId}) at {Time}", 
            article.Id, user.Username, user.Id, DateTime.Now);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article
        var article = _context.Article.FirstOrDefault(a => a.Id == id && a.AuthorId == user.Id);
        if (article == null)
        {
            return NotFound();
        }

        // Check if article can be edited
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "This article cannot be edited because it has already been reviewed.";
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
    public async Task<IActionResult> Edit(ArticleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article
        var article = _context.Article.FirstOrDefault(a => a.Id == model.Id && a.AuthorId == user.Id);
        if (article == null)
        {
            return NotFound();
        }

        // Check if article can be edited
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "This article cannot be edited because it has already been reviewed.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Update the article
        article.Title = model.Title;
        article.Abstract = model.Abstract;
        article.Content = model.Content;
        article.Topic = model.Topic;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Article {Id} updated by {Username} at {Time}", 
            article.Id, user.Username, DateTime.Now);

        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult Details(int id)
    {
        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article
        var article = _context.Article.FirstOrDefault(a => a.Id == id && a.AuthorId == user.Id);
        if (article == null)
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
        // Get the current user's username
        string username = User.Identity?.Name ?? string.Empty;
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get the article
        var article = _context.Article.FirstOrDefault(a => a.Id == id && a.AuthorId == user.Id);
        if (article == null)
        {
            return NotFound();
        }

        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "This article cannot be canceled because it has already been reviewed.";
            return RedirectToAction(nameof(Dashboard), new { page });
        }

        ArticleQuery.CancelArticle(article.Id);
        _logger.LogInformation("Article {Id} canceled by {Username} at {Time}", 
            article.Id, user.Username, DateTime.Now);
        
        TempData["SuccessMessage"] = "Your article has been successfully canceled.";
        
        // Get total number of articles after deletion
        int totalArticles = _context.Article.Count(a => a.AuthorId == user.Id);
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