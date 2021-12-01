using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingListMinimal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ShoppingListMinimal.Tests;

public class ApiTests : IClassFixture<ShoppingListFactory>
{
    private readonly ShoppingListFactory _factory;
    private readonly HttpClient _client;

    public ApiTests(ShoppingListFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        BadRequestPhrase = ReasonPhrases.GetReasonPhrase((int)HttpStatusCode.BadRequest);
        NotFoundPhrase = ReasonPhrases.GetReasonPhrase((int)HttpStatusCode.NotFound);
        ConflictPhrase = ReasonPhrases.GetReasonPhrase((int)HttpStatusCode.Conflict);
    }

    private string BadRequestPhrase { get; }
    private string NotFoundPhrase { get; }
    private string ConflictPhrase { get; }

    public async Task ResetDatabase()
    {
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();
        await Utilities.ReinitializeDatabaseForTestsAsync(dbContext);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(2)]
    public async Task GetItems_ShouldGetXItems_WhenValid(int takeItems)
    {
        // Arrange
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        var itemsInDatabase = await dbContext.Items
            .OrderByDescending(i => i.Complete)
            .Take(takeItems)
            .ToListAsync();

        // Act
        var response = await _client.GetAsync($"/items?limit={takeItems}");
        var items = await response.ReadAsAsync<IList<Item>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        items.Should().BeEquivalentTo(itemsInDatabase, o => o.WithoutStrictOrdering());
    }

    [Fact]
    public async Task GetItems_ShouldReturn400_WhenLimitParamIsInvalid()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync($"/items?limit=fail");
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Be(BadRequestPhrase);
    }

    [Fact]
    public async Task PostItems_ShouldReturn200_WhenValid()
    {
        // Arrange
        var newItem = new Item()
        {
            Name = "Water",
            Quantity = 19,
            Created = new DateTime(2021, 11, 22),
            Complete = true
        };


        // Act
        var response = await _client.PostAsJsonAsync("/items", newItem);
        var addedItem = await response.ReadAsAsync<Item>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        addedItem.Should().BeEquivalentTo(newItem, o => o.Excluding(o => o.Id));
        response.Headers.Location.Should().BeEquivalentTo(new Uri($"/items/{addedItem.Id}", UriKind.Relative));

        // Cleanup
        await ResetDatabase();
    }

    [Fact]
    public async Task PostItems_ShouldReturn400_WhenInvalidPropertiesAreSend()
    {
        // Arrange
        var newItem = new
        {
            Name = "Water",
            Quantity = 19,
            Created = "11.06.2021",
            Complete = true
        };


        // Act
        var response = await _client.PostAsJsonAsync("/items", newItem);
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Be(BadRequestPhrase);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetItems_ShouldReturnItemWithIdX_WhenCalledValid(long id)
    {
        // Arrange
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        var itemFromDatabase = await dbContext.Items.SingleAsync(i => i.Id == id);

        // Act
        var response = await _client.GetAsync($"/items/{id}");
        var item = await response.ReadAsAsync<Item>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        item.Should().BeEquivalentTo(itemFromDatabase);
    }

    [Fact]
    public async Task GetItems_ShouldReturn404_WhenIdDoesNotExist()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync($"/items/4158461");
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be(NotFoundPhrase);
    }

    [Fact]
    public async Task PutItems_ShouldReturn200_WhenIdDoesExist()
    {
        // Arrange
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        var id = 1;
        var itemFromDatabase = await dbContext.Items.SingleAsync(i => i.Id == id);

        var modifiedItem = new Item()
        {
            Id = id,
            Name = "Bread",
            Quantity = 6,
            Created = itemFromDatabase.Created,
            Complete = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{id}", modifiedItem);
        var item = await response.ReadAsAsync<Item>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        item.Should().NotBeEquivalentTo(itemFromDatabase);
        item.Should().BeEquivalentTo(modifiedItem);

        var updateItemFromDatabase = await dbContext.Items.SingleAsync(i => i.Id == id);
        item.Should().BeEquivalentTo(updateItemFromDatabase);

        // Cleanup
        await ResetDatabase();
    }

    [Fact]
    public async Task PutItems_ShouldReturn405_WhenIdsAreMismatched()
    {
        // Arrange
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        var id = 1;
        var routeId = 3;
        var itemFromDatabase = await dbContext.Items.SingleAsync(i => i.Id == id);

        var modifiedItem = new Item()
        {
            Id = id,
            Name = "Bread",
            Quantity = 6,
            Created = itemFromDatabase.Created,
            Complete = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{routeId}", modifiedItem);
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        apiResponse.Message.Should().Be(ConflictPhrase);

        // Item should stay the same
        var updateItemFromDatabase = await dbContext.Items.SingleAsync(i => i.Id == id);
        itemFromDatabase.Should().BeEquivalentTo(updateItemFromDatabase);
    }

    [Fact]
    public async Task PutItems_ShouldReturn400_WhenBodyIsInvalid()
    {
        // Arrange
        var id = 1;
        var modifiedItem = new
        {
            Id = id,
            Name = "Bread",
            Quantity = 6,
            Created = "11.06.2021",
            Complete = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{id}", modifiedItem);
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Be(BadRequestPhrase);
    }

    [Fact]
    public async Task PutItems_ShouldReturn404_WhenIdDoesNotExist()
    {
        // Arrange
        var id = 65464;
        var modifiedItem = new Item()
        {
            Id = id,
            Name = "Cheese",
            Quantity = 1,
            Created = new DateTime(2021, 11, 22),
            Complete = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{id}", modifiedItem);
        var apiResponse = await response.ReadAsAsync<ApiResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        apiResponse.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be(NotFoundPhrase);
    }

    [Fact]
    public async Task DeleteItems_ShouldReturn200_WhenIdExistsAndDeleted()
    {
        // Arrange
        var id = 2;

        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        var deletedItem = await dbContext.Items.SingleAsync(i => i.Id == id);

        // Act
        var response = await _client.DeleteAsync($"/items/{id}");
        var item = await response.ReadAsAsync<Item>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        item.Should().BeEquivalentTo(deletedItem);

        var allItems = dbContext.Items.ToList();
        allItems.Should().NotContain(deletedItem);

        // Cleanup
        await ResetDatabase();
    }

    [Fact]
    public async Task DeleteItems_ShouldReturn200_WhenIdDoesNotExist()
    {
        // Arrange
        var id = 4812;

        // Act
        var response = await _client.DeleteAsync($"/items/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
