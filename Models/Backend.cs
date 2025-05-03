class Backend {
    public static void Start() {
        QDatabase.Init("PC", "DbDotnet");

        Query q = new (Tbl.Article);
        QDatabase.Exec(conn => Console.WriteLine(q.Count(conn)));
    }
}