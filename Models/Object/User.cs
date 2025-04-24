namespace SciArticle.Models.Object;
class UserRole {
    public const string Admin = "Admin";
    public const string Author = "Author";
}
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Birthday {get;set;} = DateOnly.FromDateTime(DateTime.Now);
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.Author;
}
