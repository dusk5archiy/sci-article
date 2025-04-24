namespace SciArticle.Models.Object;
public class InsertUserForm
{
    public string Name { get; set; } = string.Empty;
    public DateOnly Birthday { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.Author;

    public void Submit()
    {
        var user = new User
        {
            Name = Name,
            Birthday = Birthday,
            Email = Email,
            Username = Username,
            Password = Password,
            Role = Role
        };

        Query q = new(Tbl.User);
        QDatabase.Exec(conn =>
        {
            user.Id = IdCounterQuery.GetIdThenIncrement(conn, Tbl.User.ToString());
            q.Insert(conn, user.ToString());
        });
    }
}