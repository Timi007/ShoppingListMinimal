using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListMinimal.Model;

public partial class Item
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime Created { get; set; }
    public bool Complete { get; set; }
}
