using SciArticle.Models.Front.User;
using SciArticle.Models.Utilities;
using SciArticle.Models.Object;

namespace SciArticle.Models.Back;
public static class UserQuery
{
    public static User CreateUser(RegisterViewModel form)
    {
        int nextId = 0;
        User user = new();
        Query q = new(Tbl.User);
        QDatabase.Exec(conn =>
        {
            nextId = IdCounterQuery.GetIdThenIncrement(conn, Tbl.User);
            user = new()
            {
                Id = nextId,
                Name = form.Name,
                Username = form.Username,
                Email = form.Email,
                Password = PasswordHasher.HashPassword(form.Password),
                Birthday = form.Birthday,
                Role = UserRole.Author // Ensure this is always an Author role
            };
            q.Insert(conn, user.ToString());
        }
        );
        return user;
    }

    public static User? GetUserByUsername(string username)
    {
        User? user = null;

        Query q = new(Tbl.User);
        q.Where(Field.User__Username, username);
        QDatabase.Exec(conn => q.Select(conn, reader => user = QDataReader.getDataObj<User>(reader)));
        return user;
    }

    public static User GetUserById(int id)
    {
        User user = new();
        Query q = new(Tbl.User);
        q.Where(Field.User__Id, id);
        QDatabase.Exec( conn => q.Select(conn, reader => user = QDataReader.getDataObj<User>(reader)));
        return user;
    }

    public static bool UsernameExists(string username)
    {
        int count = 0;
        Query q = new(Tbl.User);
        q.Where(Field.User__Username, username);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count > 0;
    }

    public static bool EmailExists(string email)
    {
        int count = 0;
        Query q = new(Tbl.User);
        q.Where(Field.User__Email, email);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count > 0;
    }

    public static List<User> GetAuthorUsers(int page, int pageSize)
    {
        List<User> users = [];
        Query q = new(Tbl.User);
        q.Where(Field.User__Role, UserRole.Author);
        q.OrderBy(Field.User__Name);
        q.Offset(page, pageSize);
        QDatabase.Exec(conn => q.Select(conn, reader => users.Add(QDataReader.getDataObj<User>(reader))));
        return users;
    }

    public static int GetAuthorCount()
    {
        int count = 0;
        Query q = new(Tbl.User);
        q.Where(Field.User__Role, UserRole.Author);

        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }
}