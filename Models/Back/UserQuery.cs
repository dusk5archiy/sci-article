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
            Query q = new(Tbl.User);
            q.Insert(conn, string.Join(", ", user.ToList()));
        }
        QDatabase.Exec(func);
        return user;
    }

    public static User? GetUserByUsername(string username)
    {
        User? user = null;

        Query q = new(Tbl.User);
        q.Where(Field.User__Username, username);
        void func(SqlConnection conn)
        {
            q.Select(conn, reader => user = QDataReader.getDataObj<User>(reader));
        }

        QDatabase.Exec(func);
        return user;
    }

    public static User GetUserById(int id)
    {
        User user = new();

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.User);
            q.Where(Field.User__Id, id);
            q.Select(conn, reader =>
            user = QDataReader.getDataObj<User>(reader));
        }

        QDatabase.Exec(func);
        return user;
    }

    public static bool UsernameExists(string username)
    {
        int count = 0;

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.User);
            q.Where(Field.User__Username, username);
            count = q.Count(conn);
        }

        QDatabase.Exec(func);
        return count > 0;
    }

    public static bool EmailExists(string email)
    {
        int count = 0;

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.User);
            q.Where(Field.User__Email, email);
            q.Output(QPiece.countAll);
            count = q.Scalar(conn);
        }

        QDatabase.Exec(func);
        return count > 0;
    }

    public static List<User> GetAuthorUsers(int page, int pageSize)
    {
        List<User> users = [];

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.User);
            q.Where(Field.User__Role, UserRole.Author);
            q.OrderBy(Field.User__Name);
            q.Offset(page, pageSize);
            q.Select(conn, reader => users.Add(QDataReader.getDataObj<User>(reader)));
        }

        QDatabase.Exec(func);
        return users;
    }

    public static int GetAuthorCount()
    {
        int count = 0;

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.User);
            q.Where(Field.User__Role, UserRole.Author);
            q.Output(QPiece.countAll);
            count = q.Scalar(conn);
        }

        QDatabase.Exec(func);
        return count;
    }
}