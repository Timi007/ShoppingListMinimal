using Microsoft.EntityFrameworkCore;
using ShoppingListMinimal;

// Setup application
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddShoppingListApi();

// Create the app
var app = builder.Build();

app.UseShoppingListExceptionHandler();

app.MapShoppingListApiRoutes();

app.Run();