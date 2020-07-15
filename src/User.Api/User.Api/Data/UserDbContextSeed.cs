using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using User.Api.Models;

namespace User.Api.Data
{
    public class UserDbContextSeed
    {
        private UserManager<AppUser> _userManager;
        public async Task SeedAsync(UserDbContext context, IServiceProvider service)
        {
            if (!context.AppUsers.Any())
            {
                await context.AppUsers.AddAsync(new AppUser
                {
                    Name = "jess"
                });
                await context.SaveChangesAsync();
            }
        }
    }
}