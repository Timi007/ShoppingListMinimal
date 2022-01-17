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

        var connectionString = builder.Configuration.GetConnectionString("ShoppingList");

        builder.Services.AddDbContext<ShoppingListContext>(options =>
           options.UseNpgsql(connectionString));

        builder.Services.AddHealthChecks()
            .AddNpgSql(connectionString);

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
            var limit = 100;
            if (context.Request.Query.TryGetValue("limit", out var limitString))
            {
                if (!int.TryParse(limitString, out limit))
                {
                    throw new StatusCodeException(StatusCodes.Status400BadRequest, "Could not parse limit query parameter to integer");
                }
            }

            var items = await dbContext.Items
                .OrderByDescending(item => item.Created)
                .Take(limit)
                .ToListAsync();

            return Results.Ok(items);
        });

        // POST /items
        builder.MapPost("/items", async (ShoppingListContext dbContext, Item shoppingListItem) =>
        {
            if (shoppingListItem.Name == null)
            {
                throw new StatusCodeException(StatusCodes.Status400BadRequest, "Item name must be set");
            }

            if (shoppingListItem.Created == default)
            {
                shoppingListItem.Created = DateTime.UtcNow;
            }

            await dbContext.AddAsync(shoppingListItem);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/items/{shoppingListItem.Id}", shoppingListItem);
        });

        // GET /items/{itemId}
        builder.MapGet("/items/{itemId}", async (long itemId, ShoppingListContext dbContext) =>
        {
            var item = await dbContext.Items
                .Where(item => item.Id == itemId)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                throw new StatusCodeException(StatusCodes.Status404NotFound, $"Item with id {itemId} does not exist.");
            }

            return Results.Ok(item);
        });

        // PUT /items/{itemId}
        builder.MapPut("/items/{itemId}", async (long itemId, Item updatedItem, ShoppingListContext dbContext) =>
        {
            var item = await dbContext.Items
                .Where(item => item.Id == itemId)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                throw new StatusCodeException(StatusCodes.Status404NotFound, $"Item with id {itemId} does not exist.");
            }

            if (itemId != updatedItem.Id)
            {
                throw new StatusCodeException(StatusCodes.Status409Conflict, $"Item id {itemId} in path does not match with id {updatedItem.Id} in body.");
            }

            item.Name = updatedItem.Name;
            item.Quantity = updatedItem.Quantity;
            item.Complete = updatedItem.Complete;
            item.Created = updatedItem.Created;

            await dbContext.SaveChangesAsync();

            return Results.Ok(item);
        });

        // DELETE /items/{itemId}
        builder.MapDelete("/items/{itemId}", async (long itemId, ShoppingListContext dbContext) =>
        {

            var item = await dbContext.Items
                .Where(item => item.Id == itemId)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return Results.Ok();
            }

            dbContext.Remove(item);
            await dbContext.SaveChangesAsync();

            return Results.Ok(item);
        });

        // Kill switch
        builder.MapGet("/fail", () =>
        {
            Environment.FailFast("A catastrophic failure in the shopping list has occurred.");
        });

        // Endpoint not found
        builder.MapFallback((HttpContext context) =>
        {
            throw new StatusCodeException(StatusCodes.Status404NotFound, $"Could not find resource \"{context.Request.Path}\"");
        });

        return builder;
    }
}
