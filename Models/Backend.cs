class Backend {
    public static void Start() {
        QDatabase.Init("PC", "DbDemo");

        Query q = new (Tbl.Article);
        QDatabase.Exec(conn => Console.WriteLine(q.Count(conn)));
    }
}