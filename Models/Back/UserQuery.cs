using SciArticle.Models.Front.User;
using SciArticle.Models.Utilities;
using SciArticle.Models.Object;
using Microsoft.Data.SqlClient;

namespace SciArticle.Models.Back;
public static class UserQuery
{
    public static User CreateUser(RegisterViewModel form)
    {
        int nextId = 0;
        User user = new();
        void func(SqlConnection conn)
        {
            nextId = IdCounterQuery.GetIdThenIncrement(conn, Tbl.User);
            user = new ()
            {
                Id = nextId,
                Name = form.Name,
                Username = form.Username,
                Email = form.Email,
                Password = PasswordHasher.HashPassword(form.Password),
                Birthday = form.Birthday,
                Role = UserRole.Author // Ensure this is always an Author role
            };
            Query q = new(Tbl.User);
            q.Insert(conn, string.Join(", ",user.ToList()));
        }
        QDatabase.Exec(func);
        return user;
    }
}