using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HouseOfNaksh.Infrastructure;

public class HouseOfNakshDbContextFactory : IDesignTimeDbContextFactory<HouseOfNakshDbContext>
{
    public HouseOfNakshDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("HOUSEOFNAKSH_MIGRATIONS_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=HouseOfNaksh;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<HouseOfNakshDbContext>()
        .UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(20),
                errorNumbersToAdd: null);
            sql.CommandTimeout(180);
        })
        .Options;

        return new HouseOfNakshDbContext(options);
    }
}