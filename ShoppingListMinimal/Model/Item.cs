using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListMinimal.Model
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime Created { get; set; }
        public bool Complete { get; set; }
    }
}
