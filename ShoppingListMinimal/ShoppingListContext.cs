using Microsoft.EntityFrameworkCore;
using ShoppingListMinimal.Model;

namespace ShoppingListMinimal;

public partial class ShoppingListContext : DbContext
{
    public ShoppingListContext()
    {
    }

    public ShoppingListContext(DbContextOptions<ShoppingListContext> options): base(options)
    {
    }

    public virtual DbSet<Item> Items { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("item");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Complete).HasColumnName("complete");

            entity.Property(e => e.Created)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");

            entity.Property(e => e.Name).HasColumnName("name");

            entity.Property(e => e.Quantity).HasColumnName("quantity");
        });

        modelBuilder.HasSequence("item_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
