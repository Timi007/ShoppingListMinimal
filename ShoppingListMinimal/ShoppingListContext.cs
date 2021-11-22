using Microsoft.EntityFrameworkCore;
using ShoppingListMinimal.Model;

namespace ShoppingListMinimal
{
    public class ShoppingListContext : DbContext
    {
        public ShoppingListContext(DbContextOptions<ShoppingListContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
    }
}
