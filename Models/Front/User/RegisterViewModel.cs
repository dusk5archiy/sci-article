using System.ComponentModel.DataAnnotations;

namespace SciArticle.Models.Front.User;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải có độ dài ít nhất {2} kí tự", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh không được để trống")]
    [DataType(DataType.Date)]
    public DateOnly Birthday { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
}