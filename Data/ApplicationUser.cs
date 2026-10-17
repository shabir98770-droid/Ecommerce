using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfilePicture { get; set; }
        public string? FullName { get; set; } 
    }
}