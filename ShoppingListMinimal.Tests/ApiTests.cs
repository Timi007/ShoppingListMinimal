using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ShoppingListMinimal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ShoppingListMinimal.Tests;

public class ApiTests: IClassFixture<ShoppingListFactory>
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
    }
     
    [Theory]
    [InlineData(100)]
    [InlineData(2)]
    public async Task GetItems_Should_GetXItemsInDB(int takeItems)
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
        var jsonString = await response.Content.ReadAsStringAsync();
        var items = JsonConvert.DeserializeObject<IList<Item>>(jsonString) ?? throw new Exception("Could not deserialize object.");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        items.Should().BeEquivalentTo(itemsInDatabase);
    }
}
