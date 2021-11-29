using ShoppingListMinimal.Model;
using System;
using System.Collections.Generic;

namespace ShoppingListMinimal.Tests;

internal class Utilities
{
    public static void InitializeDatabaseForTests(ShoppingListContext dbContext)
    {
        dbContext.Items.AddRange(GetTestItems());
        dbContext.SaveChanges();
    }

    public static void ReinitializeDatabaseForTests(ShoppingListContext dbContext)
    {
        dbContext.Items.RemoveRange(dbContext.Items);
        InitializeDatabaseForTests(dbContext);
    }

    public static IEnumerable<Item> GetTestItems()
    {
        return new List<Item>() 
        { 
            new Item(){Name = "Milk", Quantity = 3, Complete = false, Created = new DateTime(2021, 11, 25)},
            new Item(){Name = "Sugar", Quantity = 1, Complete = true, Created = new DateTime(2021, 10, 10)},
            new Item(){Name = "Coke", Quantity = 6, Complete = false, Created = new DateTime(2021, 10, 6)},
        };
    }
}
