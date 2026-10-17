using System.Collections.Generic;
using Ecommerce.Models;

namespace Ecommerce.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
    }
}