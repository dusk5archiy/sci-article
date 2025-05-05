using Microsoft.Data.SqlClient;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;
using SciArticle.Models.Front.Admin;

namespace SciArticle.Models.Back;

public static class ArticleQuery
{
    public static void CancelArticle(int id)
    {
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, id);
        QDatabase.Exec(q.Delete);
    }

    public static Article CreateArticle(ArticleEditViewModel form, int authorId)
    {
        int nextId = 0;
        Article article = new();
        QDatabase.Exec(conn =>
        {
            nextId = IdCounterQuery.GetIdThenIncrement(conn, Tbl.Article);
            article = new()
            {
                Id = nextId,
                Title = form.Title,
                Abstract = form.Abstract,
                Content = form.Content,
                Topic = form.Topic,
                AuthorId = authorId,
                TimeStamp = DateOnly.FromDateTime(DateTime.Now),
                Status = ArticleStatus.Pending
            };
            Query q = new(Tbl.Article);
            q.Insert(conn, article.ToString());
        }
        );
        return article;
    }

    public static void EditArticle(ArticleEditViewModel form)
    {
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, form.Id);
        q.SetNString(Field.Article__Title, form.Title);
        q.SetNString(Field.Article__Abstract, form.Abstract);
        q.SetNString(Field.Article__Content, form.Content);
        q.SetNString(Field.Article__Topic, form.Topic);

        QDatabase.Exec(q.Update);
    }

    public static void ApproveArticle(int id)
    {
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, id);
        q.Set(Field.Article__Status, ArticleStatus.Approved);
        QDatabase.Exec(q.Update);
    }

    public static void RejectArticle(int id)
    {
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, id);
        q.Set(Field.Article__Status, ArticleStatus.Rejected);
        QDatabase.Exec(q.Update);
    }

    public static List<Article> GetArticlesByAuthor(int authorId, int page, int pageSize)
    {
        List<Article> articles = [];

        Query q = new(Tbl.Article);
        q.Where(Field.Article__AuthorId, authorId);
        q.OrderBy(Field.Article__Id, desc: true);
        q.Offset(page, pageSize);
        QDatabase.Exec(conn => q.Select(conn, reader => articles.Add(QDataReader.getDataObj<Article>(reader))));
        return articles;
    }

    public static int GetArticleCountByAuthor(int authorId)
    {
        int count = 0;
        Query q = new(Tbl.Article);
        q.Where(Field.Article__AuthorId, authorId);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }

    public static Article? GetArticleById(int id)
    {
        Article? article = null;

        Query q = new(Tbl.Article);
        q.Where(Field.Article__Id, id);
        QDatabase.Exec(conn => q.Select(conn, reader => article = QDataReader.getDataObj<Article>(reader)));
        return article;
    }

    public static List<Article> GetArticlesByStatus(string status, int page, int pageSize)
    {
        List<Article> articles = new();

        Query q = new(Tbl.Article);
        if (status != "All")
        {
            q.Where(Field.Article__Status, status);
        }
        q.OrderBy(Field.Article__Id, desc: true);
        q.Offset(page, pageSize);
        QDatabase.Exec(conn => q.Select(conn, reader => articles.Add(QDataReader.getDataObj<Article>(reader))));
        return articles;
    }

    public static int GetArticleCountByStatus(string status)
    {
        int count = 0;

        Query q = new(Tbl.Article);
        if (status != "All")
        {
            q.Where(Field.Article__Status, status);
        }

        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }

    public static List<AuthorArticleCountViewModel> GetAuthorArticleStatistics(int page, int pageSize, string sortBy, string sortOrder)
    {
        List<AuthorArticleCountViewModel> authorStats = [];

        Query q = new(Tbl.User);
        q.Output(Field.User__Id);
        q.Output(Field.User__Name);

        Query countQ = new(Tbl.Article);
        countQ.WhereField(Field.Article__AuthorId, Field.User__Id);
        countQ.Output(QPiece.countAll);

        Query approvedQ = new(Tbl.Article);
        approvedQ.Where(Field.Article__Status, ArticleStatus.Approved);
        approvedQ.WhereField(Field.Article__AuthorId, Field.User__Id);
        approvedQ.Output(QPiece.countAll);

        Query pendingQ = new(Tbl.Article);
        pendingQ.Where(Field.Article__Status, ArticleStatus.Pending);
        pendingQ.WhereField(Field.Article__AuthorId, Field.User__Id);
        pendingQ.Output(QPiece.countAll);

        Query rejectedQ = new(Tbl.Article);
        rejectedQ.Where(Field.Article__Status, ArticleStatus.Rejected);
        rejectedQ.WhereField(Field.Article__AuthorId, Field.User__Id);
        rejectedQ.Output(QPiece.countAll);

        q.OutputQuery(countQ.SelectQuery(), "[ArticleCount]");
        q.OutputQuery(approvedQ.SelectQuery(), "[ApprovedCount]");
        q.OutputQuery(pendingQ.SelectQuery(), "[PendingCount]");
        q.OutputQuery(rejectedQ.SelectQuery(), "[RejectedCount]");
        q.Offset(page, pageSize);

        switch (sortBy)
        {
            case "AuthorName":
                q.OrderBy(Field.User__Name, sortOrder != "asc");
                break;
            case "ArticleCount" or "ApprovedCount" or "PendingCount" or "RejectedCount":
                q.OrderBy($"[{sortBy}]", sortOrder != "asc");
                break;
            default:
                q.OrderBy("[ArticleCount]", sortOrder != "asc");
                break;
        }

        QDatabase.Exec(conn => q.Select(conn, reader =>
        {
            int pos = 0;
            AuthorArticleCountViewModel authorStat = new()
            {
                AuthorId = QDataReader.getInt(reader, ref pos),
                AuthorName = QDataReader.getStr(reader, ref pos),
                ArticleCount = QDataReader.getInt(reader, ref pos),
                ApprovedCount = QDataReader.getInt(reader, ref pos),
                PendingCount = QDataReader.getInt(reader, ref pos),
                RejectedCount = QDataReader.getInt(reader, ref pos)
            };

            authorStats.Add(authorStat);
        }));

        return authorStats;
    }

    public static int GetAuthorWithArticlesCount()
    {
        int count = 0;
        Query q = new(Tbl.Article);
        q.OutputClause($"DISTINCT {Field.Article__AuthorId}");
        q.GroupBy(Field.Article__AuthorId);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }

    public static List<TopicArticleCountViewModel> GetTopicArticleStatistics(int page, int pageSize, string sortBy, string sortOrder)
    {
        List<TopicArticleCountViewModel> topicStats = [];

        Query q = new(Tbl.Article);
        q.Output($"DISTINCT {Field.Article__Topic}");

        Query countQ = new(Tbl.Article, "r");
        countQ.WhereField(Field.Article__Topic, Field.Article__Topic, "r");
        countQ.Output(QPiece.countAll);

        Query approvedQ = new(Tbl.Article, "r");
        approvedQ.Where(Field.Article__Status, ArticleStatus.Approved, "r");
        approvedQ.WhereField(Field.Article__Topic, Field.Article__Topic, "r");
        approvedQ.Output(QPiece.countAll);

        Query pendingQ = new(Tbl.Article, "r");
        pendingQ.Where(Field.Article__Status, ArticleStatus.Pending, "r");
        pendingQ.WhereField(Field.Article__Topic, Field.Article__Topic, "r");
        pendingQ.Output(QPiece.countAll);

        Query rejectedQ = new(Tbl.Article, "r");
        rejectedQ.Where(Field.Article__Status, ArticleStatus.Rejected, "r");
        rejectedQ.WhereField(Field.Article__Topic, Field.Article__Topic, "r");
        rejectedQ.Output(QPiece.countAll);

        q.OutputQuery(countQ.SelectQuery(), "[ArticleCount]");
        q.OutputQuery(approvedQ.SelectQuery(), "[ApprovedCount]");
        q.OutputQuery(pendingQ.SelectQuery(), "[PendingCount]");
        q.OutputQuery(rejectedQ.SelectQuery(), "[RejectedCount]");
        q.Offset(page, pageSize);

        switch (sortBy)
        {
            case "TopicName":
                q.OrderBy(Field.Article__Topic, sortOrder != "asc");
                break;
            case "ArticleCount" or "ApprovedCount" or "PendingCount" or "RejectedCount":
                q.OrderBy($"[{sortBy}]", sortOrder != "asc");
                break;
            default:
                q.OrderBy("[ArticleCount]", sortOrder != "asc");
                break;
        }

        QDatabase.Exec(conn => q.Select(conn, reader =>
        {
            int pos = 0;
            TopicArticleCountViewModel topicStat = new()
            {
                TopicName = QDataReader.getStr(reader, ref pos),
                ArticleCount = QDataReader.getInt(reader, ref pos),
                ApprovedCount = QDataReader.getInt(reader, ref pos),
                PendingCount = QDataReader.getInt(reader, ref pos),
                RejectedCount = QDataReader.getInt(reader, ref pos)
            };

            topicStats.Add(topicStat);
        }));

        return topicStats;
    }

    public static int GetTopicCount()
    {
        int count = 0;
        List<string> topics = new();

        void func(SqlConnection conn)
        {
            Query q = new(Tbl.Article);
            q.Output($"DISTINCT {Field.Article__Topic}");
            q.Select(conn, reader =>
            {
                if (!reader.IsDBNull(0))
                {
                    topics.Add(reader.GetString(0));
                }
            });

            count = topics.Count;
        }

        QDatabase.Exec(func);
        return count;
    }

    public static int GetTotalArticleCount()
    {
        int count = 0;

        Query q = new(Tbl.Article);
        q.Output(QPiece.countAll);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }

    public static int GetArticleCountByStatusType(string status)
    {
        int count = 0;
        Query q = new(Tbl.Article);
        q.Where(Field.Article__Status, status);
        q.Output(QPiece.countAll);
        QDatabase.Exec(conn => count = q.Count(conn));
        return count;
    }

    public static List<AdminArticleViewModel> GetArticlesForAdminDashboard(string statusFilter, int page, int pageSize)
    {
        List<AdminArticleViewModel> articleViewModels = [];
        Query articleQ = new(Tbl.Article);
        articleQ.Output(Field.Article__Id);
        articleQ.Output(Field.Article__Title);
        articleQ.Output(Field.Article__AuthorId);
        articleQ.Output(Field.Article__TimeStamp);
        articleQ.Output(Field.Article__Topic);
        articleQ.Output(Field.Article__Status);
        
        articleQ.Join(Field.User__Id, Field.Article__AuthorId);
        articleQ.Output(Field.User__Name);
        
        if (statusFilter != "All")
        {
            articleQ.Where(Field.Article__Status, statusFilter);
        }
        
        articleQ.OrderBy(Field.Article__Id, desc: true);
        articleQ.Offset(page, pageSize);
        
        QDatabase.Exec(conn => articleQ.Select(conn, reader =>
        {
            int pos = 0;
            AdminArticleViewModel viewModel = new()
            {
                Id = QDataReader.getInt(reader, ref pos),
                Title = QDataReader.getStr(reader, ref pos),
                AuthorId = QDataReader.getInt(reader, ref pos),
                TimeStamp = QDataReader.getDate(reader, ref pos),
                Topic = QDataReader.getStr(reader, ref pos),
                Status = QDataReader.getStr(reader, ref pos),
                AuthorName = QDataReader.getStr(reader, ref pos)
            };
            
            articleViewModels.Add(viewModel);
        }));
        
        return articleViewModels;
    }

    public static List<AuthorArticleViewModel> GetArticlesForAuthorDashboard(int authorId, int page, int pageSize)
    {
        List<AuthorArticleViewModel> articleViewModels = [];

        Query articleQ = new(Tbl.Article);
        articleQ.Output(Field.Article__Id);
        articleQ.Output(Field.Article__Title);
        articleQ.Output(Field.Article__TimeStamp);
        articleQ.Output(Field.Article__Topic);
        articleQ.Output(Field.Article__Status);
        
        articleQ.Where(Field.Article__AuthorId, authorId);
        
        articleQ.OrderBy(Field.Article__Id, desc: true);
        articleQ.Offset(page, pageSize);
        
        QDatabase.Exec(conn => articleQ.Select(conn, reader =>
        {
            int pos = 0;
            AuthorArticleViewModel viewModel = new()
            {
                Id = QDataReader.getInt(reader, ref pos),
                Title = QDataReader.getStr(reader, ref pos),
                TimeStamp = QDataReader.getDate(reader, ref pos),
                Topic = QDataReader.getStr(reader, ref pos),
                Status = QDataReader.getStr(reader, ref pos)
            };
            
            articleViewModels.Add(viewModel);
        }));
        
        return articleViewModels;
    }
}