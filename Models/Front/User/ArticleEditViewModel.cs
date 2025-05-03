using System.ComponentModel.DataAnnotations;

namespace SciArticle.Models.Front.User;

public class ArticleEditViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    [StringLength(100, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} kí tự.", MinimumLength = 5)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Tóm tắt là bắt buộc.")]
    [StringLength(500, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} kí tự.", MinimumLength = 10)]
    [Display(Name = "Tóm tắt")]
    public string Abstract { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Nội dung là bắt buộc.")]
    [StringLength(10000, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} kí tự.", MinimumLength = 50)]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Chủ đề là bắt buộc.")]
    [Display(Name = "Chủ đề")]
    public string Topic { get; set; } = string.Empty;
}