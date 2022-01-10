using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ShoppingListMinimal;

// Setup application
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.AddShoppingListApi();

builder.Services.AddHealthChecks();

// Create the app
var app = builder.Build();

app.UseShoppingListExceptionHandler();

app.MapShoppingListApiRoutes();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false
});

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}