using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ProjectTemplate.Models.Domain
{
    public class Role : IdentityRole<int>, IIdentifiable
    {
        public List<UserRole> UserRoles { get; set; }
    }
}