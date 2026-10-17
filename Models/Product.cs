using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description max 1000 characters")]
        [Display(Name = "Product Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 1000000, ErrorMessage = "Price must be between 1 and 10,00,000")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Product Image")]
        [Url(ErrorMessage = "Enter valid image URL")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; } = "Mobile";

        
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "In Stock")]
        public bool IsAvailable { get; set; } = true;

        
        [Display(Name = "Stock Quantity")]
        [Range(0, 10000)]
        public int Stock { get; set; } = 100;
    }
}