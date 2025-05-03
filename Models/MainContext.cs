using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SciArticle.Models.Object;

namespace SciArticle.Models;

public class MainContext : DbContext
{
    public required DbSet<Article> Article { get; set; } = null!;
    public required DbSet<User> User { get; set; } = null!;
    public required DbSet<IdCounter> IdCounter { get; set; } = null!;

    public MainContext() { }

    public MainContext(DbContextOptions<MainContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string conn_string = new SqlConnectionStringBuilder
        {
            DataSource = "PC",
            InitialCatalog = "DbDotnet",
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 60,
        }.ConnectionString;
        optionsBuilder.UseSqlServer(conn_string);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasNoKey();
        modelBuilder.Entity<User>().HasNoKey();
        modelBuilder.Entity<IdCounter>().HasNoKey();
    }
}