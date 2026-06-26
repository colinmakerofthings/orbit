using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orbit.Persistence;

public class OrbitDbContextFactory : IDesignTimeDbContextFactory<OrbitDbContext>
{
    public OrbitDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrbitDbContext>()
            .UseSqlite("Data Source=orbit-design.db")
            .Options;

        return new OrbitDbContext(options);
    }
}
