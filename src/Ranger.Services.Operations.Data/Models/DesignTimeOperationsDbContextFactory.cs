using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.Services.Operations.Data {
    public class DesignTimeOperationDbContextFactory : IDesignTimeDbContextFactory<OperationsDbContext> {
        public OperationsDbContext CreateDbContext (string[] args) {
            var config = new ConfigurationBuilder ()
                .SetBasePath (System.IO.Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json")
                .Build ();

            var options = new DbContextOptionsBuilder<OperationsDbContext> ();
            options.UseNpgsql (config["CloudSql:OperationsConnectionString"]);

            return new OperationsDbContext (options.Options);
        }
    }
}