using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ShoppingListMinimal;

// Setup application
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.AddShoppingListApi();

// Create the app
var app = builder.Build();

app.UseShoppingListExceptionHandler();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false
});

app.MapShoppingListApiRoutes();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}