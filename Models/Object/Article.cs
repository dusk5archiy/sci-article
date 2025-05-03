using System.ComponentModel.DataAnnotations;

namespace SciArticle.Models.Object;

class ArticleStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public DateOnly TimeStamp { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string Abstract { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<string> ToList() {
        List<string> fields = [
            QPiece.toStr(Id),
            QPiece.toNStr(Title),
            QPiece.toStr(AuthorId),
            QPiece.toStr(TimeStamp),
            QPiece.toNStr(Abstract),
            QPiece.toNStr(Content),
            QPiece.toNStr(Topic),
            QPiece.toStr(Status)
        ];
        return fields;
    }
    public override string ToString()
    {
        return string.Join(", ", ToList());
    }
}