using System.Collections.Generic;
using ProjectTemplate.Models.Domain;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ProjectTemplate.Data
{
    public class Seeder
    {
        private readonly DataContext context;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;


        public Seeder(DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void SeedDatabase(bool clearCurrentData = false, bool applyMigrations = false)
        {
            if (applyMigrations)
            {
                context.Database.Migrate();
            }

            if (clearCurrentData)
            {
                ClearAllData();
            }

            SeedRoles();
            SeedUsers();
        }

        private void ClearAllData()
        {
            context.RefreshTokens.RemoveRange(context.RefreshTokens);
            context.Users.RemoveRange(context.Users);
            context.Roles.RemoveRange(context.Roles);

            context.SaveChanges();
        }

        private void SeedRoles()
        {
            if (context.Roles.Any())
            {
                return;
            }

            string data = System.IO.File.ReadAllText("Data/SeedData/RoleSeedData.json");
            var roles = JsonConvert.DeserializeObject<List<Role>>(data);

            foreach (var role in roles)
            {
                roleManager.CreateAsync(role).Wait();
            }
        }

        private void SeedUsers()
        {
            if (userManager.Users.Any())
            {
                return;
            }

            string data = System.IO.File.ReadAllText("Data/SeedData/UserSeedData.json");
            List<User> users = JsonConvert.DeserializeObject<List<User>>(data);

            foreach (User user in users)
            {
                userManager.CreateAsync(user, "password").Wait();

                if (user.UserName.ToUpper() == "ADMIN")
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                    userManager.AddToRoleAsync(user, "User").Wait();
                }
                else
                {
                    userManager.AddToRoleAsync(user, "User").Wait();
                }
            }
        }
    }
}