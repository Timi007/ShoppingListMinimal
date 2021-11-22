using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ShoppingListMinimal;
using ShoppingListMinimal.Model;

// Setup application
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddDbContext<ShoppingListContext>(o => o.UseInMemoryDatabase(databaseName: "ShoppingList"));


var app = builder.Build();

// Exception handling
app.UseExceptionHandler(builder => builder.Run(async (HttpContext context) =>
{
    var exception = context.Features
        .Get<IExceptionHandlerPathFeature>()?
        .Error;

    var response = exception switch
    {
        StatusCodeException statusCodeEx => new ApiResponse
        {
            StatusCode = statusCodeEx.StatusCode,
            Message = ReasonPhrases.GetReasonPhrase(statusCodeEx.StatusCode),
            Error = statusCodeEx.Message,
        },
        BadHttpRequestException badRequestEx => new ApiResponse
        {
            StatusCode = badRequestEx.StatusCode,
            Message = ReasonPhrases.GetReasonPhrase(badRequestEx.StatusCode),
            Error = badRequestEx.InnerException?.Message ?? badRequestEx.Message,
        },
        _ => new ApiResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError),
            Error = exception?.Message ?? "Could not identify exception",
        }
    };

    app.Logger.LogError(response.Error);

    context.Response.StatusCode = response.StatusCode;
    await context.Response.WriteAsJsonAsync(response);
}));

// REST API
app.MapGet("/items", async (HttpContext context, ShoppingListContext dbContext) =>
{
    if (!context.Request.Query.TryGetValue("limit", out var limitString))
    {
        throw new StatusCodeException(StatusCodes.Status400BadRequest, "Missing limit query parameter");
    }

    if (!int.TryParse(limitString, out var limit))
    {
        throw new StatusCodeException(StatusCodes.Status400BadRequest, "Could not parse limit query parameter to integer");
    }

    return await dbContext.Items
        .OrderBy(item => item.Created)
        .Take(limit)
        .ToListAsync();
});

app.MapPost("/items", async (HttpContext context, ShoppingListContext dbContext, Item shoppingListItem) =>
{
    await dbContext.AddAsync(shoppingListItem);
    await dbContext.SaveChangesAsync();

    context.Response.StatusCode = StatusCodes.Status201Created;
    return shoppingListItem;
});

app.MapFallback((HttpContext context) =>
{
    throw new StatusCodeException(StatusCodes.Status404NotFound, $"Could not find resource \"{context.Request.Path}\"");
});


app.Run();