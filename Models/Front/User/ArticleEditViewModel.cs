using System.ComponentModel.DataAnnotations;

namespace SciArticle.Models.Front.User;

public class ArticleEditViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
    public string Abstract { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 50)]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Topic { get; set; } = string.Empty;
}