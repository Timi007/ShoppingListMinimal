using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ShoppingListMinimal.Model;

namespace ShoppingListMinimal;

public static class ApiEndpoints
{
    public static WebApplicationBuilder AddShoppingListApi(this WebApplicationBuilder builder)
    {
        //builder.Services.AddDbContext<ShoppingListContext>(o => o.UseInMemoryDatabase(databaseName: "ShoppingList"));

        builder.Services.AddDbContext<ShoppingListContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("ShoppingList")));

        return builder;
    }

    public static WebApplication UseShoppingListExceptionHandler(this WebApplication app)
    {
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

        return app;
    }

    public static IEndpointRouteBuilder MapShoppingListApiRoutes(this IEndpointRouteBuilder builder)
    {
        // GET /items
        builder.MapGet("/items", async (HttpContext context, ShoppingListContext dbContext) =>
        {
            if (!context.Request.Query.TryGetValue("limit", out var limitString))
            {
                throw new StatusCodeException(StatusCodes.Status400BadRequest, "Missing limit query parameter");
            }

            if (!int.TryParse(limitString, out var limit))
            {
                throw new StatusCodeException(StatusCodes.Status400BadRequest, "Could not parse limit query parameter to integer");
            }

            var items = await dbContext.Items
                .OrderByDescending(item => item.Created)
                .Take(limit)
                .ToListAsync();

            return Results.Ok(items);
        });

        // POST /items
        builder.MapPost("/items", async (HttpContext context, ShoppingListContext dbContext, Item shoppingListItem) =>
        {
            await dbContext.AddAsync(shoppingListItem);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/items/{shoppingListItem.Id}", shoppingListItem);
        });

        // Endpoint not found
        builder.MapFallback((HttpContext context) =>
        {
            throw new StatusCodeException(StatusCodes.Status404NotFound, $"Could not find resource \"{context.Request.Path}\"");
        });


        builder.MapGet("/items/{itemId}", async (long itemId, HttpContext context, ShoppingListContext dbContext) =>
        {

            var item = await dbContext.Items.Where(item => item.Id == itemId).FirstOrDefaultAsync();

            if (item is null)
            {
                throw new StatusCodeException(StatusCodes.Status404NotFound, "Item not found");
            }

            return Results.Ok(item);
        });

        builder.MapPut("/items/{itemId}", async (long itemId, Item updatedItem, HttpContext context, ShoppingListContext dbContext) =>
        {
            var item = await dbContext.Items.Where(item => item.Id == itemId).FirstOrDefaultAsync();

            if (item is null)
            {
                throw new StatusCodeException(StatusCodes.Status404NotFound, "Item not found");
            }

            if (item.Id != updatedItem.Id)
            {
                throw new StatusCodeException(StatusCodes.Status409Conflict, "Item id in path does not match with id in body.");
            }

            item.Name = updatedItem.Name;
            item.Quantity = updatedItem.Quantity;
            item.Complete = updatedItem.Complete;
            item.Created = updatedItem.Created;

            await dbContext.SaveChangesAsync();

            return Results.Ok(item);
        });

        builder.MapDelete("/items/{itemId}", async (int itemId, HttpContext context, ShoppingListContext dbContext) =>
        {

            var item = await dbContext.Items.Where(item => item.Id == itemId).FirstOrDefaultAsync();

            if (item != null)
            {
                dbContext.Remove(item);
            }

            await dbContext.SaveChangesAsync();
            return Results.Ok(item);
        });

        return builder;
    }
}
