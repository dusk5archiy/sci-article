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

    public List<string> ToList() {
        List<string> fields = [
            QPiece.toStr(Id),
            QPiece.toNStr(Name),
            QPiece.toStr(Birthday),
            QPiece.toStr(Email),
            QPiece.toStr(Username),
            QPiece.toStr(Password),
            QPiece.toStr(Role)
        ];
        return fields;
    }
    public override string ToString()
    {
        return string.Join(", ", ToList());
    }
}
