static class QPiece
{
    public const string FALSE = "1 = 0";

    public static string containsStr(string field, string value) => $"{field} LIKE '%{value}%'";

    public static string castFloat(string field) => $"CAST({field} AS FLOAT)";

    public static string toStr(int value) => $"{value}";

    public static string toStr(DateOnly value) => $"'{value.Year}-{value.Month:D2}-{value.Day:D2}'";

    public static string toStr(DateTime value) =>
        $"'{value.Year}-{value.Month:D2}-{value.Day:D2} {value.Hour:D2}:{value.Minute:D2}:{value.Second:D2}'";

    public static string toStr(string value) => $"\'{value}\'";

    public static string toNStr(string value) => $"N\'{value}\'";

    public static string avg(string field)
    {
        return $"ISNULL(AVG({field}), 0)";
    }

    public static string fieldAlias(string field, string? alias)
    {
        if (alias is null)
            return field;
        return $"{field} AS {alias}";
    }

    public static string tableAlias(string? table, string? alias)
    {
        if (alias is null)
            return $"[{table}]";
        return $"[{table}] AS {alias}";
    }

    public static string dot(string field, string? alias = null)
    {
        if (alias is null)
        {
            return field;
        }
        var parts = field.Split('.');
        field = parts.Count() > 1 ? parts[1] : field;
        return $"[{alias}].{field}";
    }

    public const string countAll = "COUNT(*)";

    public static string allFieldsOf(string table) => $"[{table}].*";

    // ------------------------------------------------------------------------
    public static string eq<T>(string field, T value, string op = "=")
    {
        return $"{field} {op} {value}";
    }

    public static string eq(string field, string value, string op = "=")
    {
        return $"{field} {op} '{value}'";
    }

    public static string eqField(string field, string value, string op = "=")
    {
        return $"{field} {op} {value}";
    }

    public static string eq(string field, DateOnly value, string op = "=")
    {
        return $"{field} {op} {QPiece.toStr(value)}";
    }

    public static string eq(string field, DateTime value, string op = "=")
    {
        return $"{field} {op} {QPiece.toStr(value)}";
    }

    // ------------------------------------------------------------------------
    public static string inList<T>(string field, List<T> values)
    {
        if (values.Count == 0)
            return FALSE;
        string str = string.Join(", ", values);
        return $"{field} IN ({str})";
    }

    public static string inList(string field, List<string> values)
    {
        if (values.Count == 0)
            return FALSE;
        values = values.Select(v => $"\'{v}\'").ToList();
        return inList<string>(field, values);
    }

    // ------------------------------------------------------------------------
    public static string orderBy(string field, bool desc = false)
    {
        string s = field;
        if (desc)
            s += " DESC";
        return s;
    }

    public static string join(string table_1, string field_1, string field_2)
    {
        return $"INNER JOIN {table_1} ON {field_1} = {field_2}";
    }

    public static string LeftJoin(string table_1, string field_1, string field_2)
    {
        return $"LEFT JOIN {table_1} ON {field_1} = {field_2}";
    }
}

/* EOF */
