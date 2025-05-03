using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;

namespace SciArticle.Models.Back;

public static class ArticleQuery {
    public static void CancelArticle(int id)
    {
        Query q2 = new(Tbl.Article);
        q2.Where(Field.Article__Id, id);
        QDatabase.Exec(conn => Console.WriteLine($"Number {q2.Count(conn)}"));
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, id);
        QDatabase.Exec(q.Delete);
    }

    public static Article CreateArticle(ArticleEditViewModel form, int authorId)
    {
        int nextId = 0;
        Article article = new();

        void func(SqlConnection conn)
        {
            nextId = IdCounterQuery.GetIdThenIncrement(conn, Tbl.Article);
            article = new()
            {
                Id = nextId,
                Title = form.Title,
                Abstract = form.Abstract,
                Content = form.Content,
                Topic = form.Topic,
                AuthorId = authorId, // Use the passed author ID instead of form.Id
                TimeStamp = DateOnly.FromDateTime(DateTime.Now),
                Status = ArticleStatus.Pending // New articles are always pending
            };
            Query q = new(Tbl.Article);
            q.Insert(conn, string.Join(", ", article.ToList()));
        }
        QDatabase.Exec(func);
        return article;
    }
}