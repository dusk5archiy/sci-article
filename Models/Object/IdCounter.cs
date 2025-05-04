namespace SciArticle.Models.Object;
public class IdCounter
{
    public string TableName { get; set; } = string.Empty;
    public int Count { get; set; }

    public List<string> ToList() {
        List<string> fields = [
            QPiece.toStr(TableName),
            QPiece.toStr(Count)
        ];
        return fields;
    }
    public override string ToString()
    {
        return string.Join(", ", ToList());
    }
}
