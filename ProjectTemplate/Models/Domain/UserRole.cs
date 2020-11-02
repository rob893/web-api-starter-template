using Microsoft.AspNetCore.Identity;

namespace ProjectTemplate.Models.Domain
{
    public class UserRole : IdentityUserRole<int>
    {
        public User User { get; set; }
        public Role Role { get; set; }
    }
}