using Microsoft.Data.SqlClient;

// INFO: Tập hợp các hàm dùng để tạo đối tượng từ một SqlDataReader.
// Cần xác định pos là chỉ số cột bắt đầu để đọc dữ liệu.
sealed class QDataReader
{
    // ========================================================================
    public static int getInt(SqlDataReader reader, ref int pos)
    {
        return reader.GetInt32(pos++);
    }

    public static int getInt(SqlDataReader reader, int pos = 0)
    {
        return getInt(reader, ref pos);
    }

    public static double getDouble(SqlDataReader reader, ref int pos)
    {
        return reader.GetDouble(pos++);
    }

    public static double getDouble(SqlDataReader reader, int pos = 0)
    {
        return getDouble(reader, ref pos);
    }

    // ------------------------------------------------------------------------
    public static string getStr(SqlDataReader reader, int pos = 0)
    {
        return reader.GetString(pos++);
    }

    public static string getStr(SqlDataReader reader, ref int pos)
    {
        return reader.GetString(pos++);
    }

    // ------------------------------------------------------------------------
    public static DateOnly getDate(SqlDataReader reader, ref int pos)
    {
        DateTime d = reader.GetDateTime(pos++);
        return new(d.Year, d.Month, d.Day);
    }

    public static DateTime getDateTime(SqlDataReader reader, ref int pos)
    {
        DateTime d = reader.GetDateTime(pos++);
        return d;
    }

    // ------------------------------------------------------------------------
    // public static T getEnum<T>(SqlDataReader reader, ref int pos)
    //     where T : Enum
    // {
    //     return (T)Enum.ToObject(typeof(T), reader.GetInt32(pos++));
    // }

    // ========================================================================
    public static T getDataObj<T>(SqlDataReader reader, ref int pos)
        where T : DataObj, new()
    {
        T info = new T();
        info.fetch(reader, ref pos);
        return info;
    }

    public static T getDataObj<T>(SqlDataReader reader, int pos = 0)
        where T : DataObj, new()
    {
        return getDataObj<T>(reader, ref pos);
    }

    // ========================================================================
}

/* EOF */
