using ShoppingListMinimal;

// Setup application
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.AddShoppingListApi();

// Create the app
var app = builder.Build();

app.UseShoppingListExceptionHandler();

app.MapShoppingListApiRoutes();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}