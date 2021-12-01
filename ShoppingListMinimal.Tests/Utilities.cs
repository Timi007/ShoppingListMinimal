using ShoppingListMinimal.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingListMinimal.Tests;

internal class Utilities
{
    public static void InitializeDatabaseForTests(ShoppingListContext dbContext)
    {
        dbContext.Items.AddRange(GetTestItems());
        dbContext.SaveChanges();
    }

    public static async Task InitializeDatabaseForTestsAsync(ShoppingListContext dbContext)
    {
        await dbContext.Items.AddRangeAsync(GetTestItems());
        await dbContext.SaveChangesAsync();
    }

    public static void ReinitializeDatabaseForTests(ShoppingListContext dbContext)
    {
        dbContext.Items.RemoveRange(dbContext.Items);
        dbContext.SaveChanges();
        InitializeDatabaseForTests(dbContext);
    }

    public static async Task ReinitializeDatabaseForTestsAsync(ShoppingListContext dbContext)
    {
        dbContext.Items.RemoveRange(dbContext.Items);
        await dbContext.SaveChangesAsync();
        await InitializeDatabaseForTestsAsync(dbContext);
    }

    public static IEnumerable<Item> GetTestItems()
    {
        yield return new Item() { Name = "Milk", Quantity = 3, Complete = false, Created = new DateTime(2021, 11, 25) };
        yield return new Item() { Name = "Sugar", Quantity = 1, Complete = true, Created = new DateTime(2021, 10, 10) };
        yield return new Item() { Name = "Coke", Quantity = 6, Complete = false, Created = new DateTime(2021, 10, 6) };
    }
}
