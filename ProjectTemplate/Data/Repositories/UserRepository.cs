using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Core;
using ProjectTemplate.Models.Domain;
using ProjectTemplate.Models.QueryParameters;

namespace ProjectTemplate.Data.Repositories
{
    public class UserRepository : Repository<User, CursorPaginationParameters>
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;


        public UserRepository(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager) : base(context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password)
        {
            var created = await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "User");

            return created;
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            IQueryable<User> query = context.Users;
            query = AddIncludes(query);

            return query.FirstOrDefaultAsync(user => user.UserName == username);
        }

        public Task<User> GetByUsernameAsync(string username, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = context.Users;

            query = AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return query.FirstOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);

            return result.Succeeded;
        }

        public Task<CursorPagedList<Role>> GetRolesAsync(CursorPaginationParameters searchParams)
        {
            IQueryable<Role> query = context.Roles;

            return CursorPagedList<Role>.CreateAsync(query, searchParams);
        }

        public Task<List<Role>> GetRolesAsync()
        {
            return context.Roles.ToListAsync();
        }

        protected override IQueryable<User> AddIncludes(IQueryable<User> query)
        {
            return query.Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role);
        }
    }
}