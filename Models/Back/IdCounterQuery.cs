using Microsoft.Data.SqlClient;

namespace SciArticle.Models.Back;

static class IdCounterQuery
{
    public static int GetIdThenIncrement(SqlConnection conn, string table)
    {
        Query q = new(Tbl.IdCounter);
        q.Where(Field.IdCounter__TableName, table);
        q.Output(Field.IdCounter__Count);
        int result = q.Scalar(conn);

        q = new(Tbl.IdCounter);
        q.Set(Field.IdCounter__Count, result + 1);
        q.Where(Field.IdCounter__TableName, table);
        q.Update(conn);

        return result;
    }
}