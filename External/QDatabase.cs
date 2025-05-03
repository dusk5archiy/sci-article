using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

sealed class QDatabase
{
    // ========================================================================
    private static int query_counter = 0;
    private static string server_only_conn_string = "";
    private static string default_conn_string = "";

    public static void Init(string server_name, string database_name)
    {
        server_only_conn_string = new SqlConnectionStringBuilder
        {
            DataSource = server_name,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 60,
            MultipleActiveResultSets = true,
        }.ConnectionString;

        default_conn_string = new SqlConnectionStringBuilder
        {
            DataSource = server_name,
            InitialCatalog = database_name,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 60,
        }.ConnectionString;
    }

    // ========================================================================
    // INFO: Delegate cho các hàm nhận conn làm tham số.
    public delegate void ConnFunction(SqlConnection conn);

    // ========================================================================
    // INFO: Tạo mới conn và truyền conn vào ConnFunction, sau đó ngắt kết nối.
    public static void Exec(ConnFunction conn_function, bool server_only = false)
    {
        string conn_string = server_only ? server_only_conn_string : default_conn_string;
        try
        {
            using (SqlConnection conn = new SqlConnection(conn_string))
            {
                conn.Open();
                conn_function(conn); // khả năng thì nó nhận một biểu thức lambda và thực thi một phương thức truy vấn nào đó...
            }
        }
        catch (SqlException e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    // ========================================================================
    // INFO:
    // Đây là hàm chạy trên một conn cụ thể.
    // Chạy non_query (query kiểu hành động, không trả về dữ liệu)
    public static void ExecQuery(SqlConnection conn, string query)
    {
        int counter = ++query_counter;
        Console.WriteLine($"[START] query #{counter}: {query[..Math.Min(100, query.Length)]}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        using (SqlCommand command = new SqlCommand(query, conn))
        {
            command.ExecuteNonQuery();
        }
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        Console.WriteLine(
            $"[FINISH] query #{counter} - Time taken: {elapsed.TotalMilliseconds} ms"
        );
    }

    // ========================================================================
    // INFO:
    // ReaderFunction là một hàm nhận một reader làm tham số.
    // ReaderFunction sẽ liên tục được truyền reader tương ứng cho mỗi bản ghi của kết quả
    // truy vấn, khi đó nó có thể đọc dữ liệu của mỗi bản ghi và sẽ có hành động nhất định
    // với dữ liệu đọc được. Ví dụ, một reader_function mỗi lần nhận id và name
    // mới thì sẽ thêm id và name đó vào một list kết quả.
    public delegate void ReaderFunction(SqlDataReader reader);

    // ========================================================================
    // INFO: Tạo reader và truyền reader vào reader_function.
    public static void ExecQuery(SqlConnection conn, string query, ReaderFunction f)
    {
        int counter = ++query_counter;
        Console.WriteLine($"[START] query #{counter}: {query}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        using (SqlCommand command = new SqlCommand(query, conn))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    f(reader);
                }
            }
        }
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        Console.WriteLine(
            $"[FINISH] query #{counter} - Time taken: {elapsed.TotalMilliseconds} ms"
        );
    }

    // ========================================================================
    // INFO: Async delegate for functions that accept a connection
    public delegate Task ConnFunctionAsync(SqlConnection conn);

    // ========================================================================
    // INFO: Create a new connection, pass it to ConnFunctionAsync, then disconnect
    public static async Task ExecAsync(ConnFunctionAsync conn_function, bool server_only = false)
    {
        string conn_string = server_only ? server_only_conn_string : default_conn_string;
        try
        {
            using (SqlConnection conn = new (conn_string))
            {
                await conn.OpenAsync();
                await conn_function(conn); 
            }
        }
        catch (SqlException e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    // ========================================================================
    // INFO: Async version of ExecQuery for non-query operations
    public static async Task ExecQueryAsync(SqlConnection conn, string query)
    {
        int counter = ++query_counter;
        Console.WriteLine($"[START] query #{counter}: {query[..Math.Min(100, query.Length)]}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        using (SqlCommand command = new SqlCommand(query, conn))
        {
            await command.ExecuteNonQueryAsync();
        }
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        Console.WriteLine(
            $"[FINISH] query #{counter} - Time taken: {elapsed.TotalMilliseconds} ms"
        );
    }

    // ========================================================================
    // INFO: Async delegate for functions that accept a reader
    public delegate Task ReaderFunctionAsync(SqlDataReader reader);

    // ========================================================================
    // INFO: Async version of ExecQuery with reader function
    public static async Task ExecQueryAsync(SqlConnection conn, string query, ReaderFunctionAsync f)
    {
        int counter = ++query_counter;
        Console.WriteLine($"[START] query #{counter}: {query}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        using (SqlCommand command = new SqlCommand(query, conn))
        {
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    await f(reader);
                }
            }
        }
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        Console.WriteLine(
            $"[FINISH] query #{counter} - Time taken: {elapsed.TotalMilliseconds} ms"
        );
    }

    // ========================================================================
}

/* EOF */
