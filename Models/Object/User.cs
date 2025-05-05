using Microsoft.Data.SqlClient;

namespace SciArticle.Models.Object;
class UserRole {
    public const string Admin = "Admin";
    public const string Author = "Author";
}
public class User : DataObj
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Birthday {get;set;} = DateOnly.FromDateTime(DateTime.Now);
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.Author;

    public override void fetch(SqlDataReader reader, ref int pos)
    {
        base.fetch(reader, ref pos);
        Id = QDataReader.getInt(reader, ref pos);
        Name = QDataReader.getStr(reader, ref pos);
        Birthday = QDataReader.getDate(reader, ref pos);
        Email = QDataReader.getStr(reader, ref pos);
        Username = QDataReader.getStr(reader, ref pos);
        Password = QDataReader.getStr(reader, ref pos);
        Role = QDataReader.getStr(reader, ref pos);
    }
    public override List<string> ToList() {
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
}
