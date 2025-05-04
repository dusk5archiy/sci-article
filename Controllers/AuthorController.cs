using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models.Front.User;
using SciArticle.Models.Back;
using SciArticle.Models.Object;

namespace SciArticle.Controllers;

[Authorize(Roles = UserRole.Author)]
public class AuthorController : Controller
{
    private const int ItemsPerPage = 10;
    private User GetCurrentUser()
    {
        string username = User.Identity?.Name ?? string.Empty;
        return UserQuery.GetUserByUsername(username) ?? new();
    }

    public IActionResult Dashboard(int page = 1)
    {
        var user = GetCurrentUser();
        int totalItems = ArticleQuery.GetArticleCountByAuthor(user.Id);
        page = Math.Max(1, page); 
        var articles = ArticleQuery.GetArticlesByAuthor(user.Id, page, ItemsPerPage);
        var articleViewModels = articles.Select(a => new AuthorArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            TimeStamp = a.TimeStamp,
            Topic = a.Topic,
            Status = a.Status
        }).ToList();
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
        ArticleQuery.CreateArticle(model, user.Id);
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = GetCurrentUser();
        var article = ArticleQuery.GetArticleById(id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bài báo này không thể được chỉnh sửa vì đã được xem xét.";
            return RedirectToAction(nameof(Dashboard));
        }
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
        var article = ArticleQuery.GetArticleById(model.Id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }
        if (article.Status != ArticleStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bài báo này không thể được chỉnh sửa vì đã được xem xét.";
            return RedirectToAction(nameof(Dashboard));
        }
        ArticleQuery.EditArticle(model);
        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult Details(int id)
    {
        var user = GetCurrentUser();
        var article = ArticleQuery.GetArticleById(id);
        if (article == null || article.AuthorId != user.Id)
        {
            return NotFound();
        }
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
        TempData["SuccessMessage"] = "Bài báo của bạn đã được hủy thành công.";
        
        int totalArticles = ArticleQuery.GetArticleCountByAuthor(user.Id);
        int totalPages = (int)Math.Ceiling((double)totalArticles / ItemsPerPage);
        
        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
        }
        else if (totalPages == 0)
        {
            page = 1;
        }
        
        return RedirectToAction(nameof(Dashboard), new { page });
    }
}