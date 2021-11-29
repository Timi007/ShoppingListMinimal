using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ShoppingListMinimal.Tests;

public class ShoppingListFactory: WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ShoppingListContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }


            services.AddDbContext<ShoppingListContext>(o => o.UseInMemoryDatabase(databaseName: "ShoppingListTest"));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ShoppingListContext>();

            db.Database.EnsureCreated();

            Utilities.InitializeDatabaseForTests(db);
        });
    }
}

