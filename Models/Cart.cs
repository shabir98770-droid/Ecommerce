using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

      
        public string UserId { get; set; } = string.Empty;

        
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

       
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}