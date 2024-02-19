using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //ROLES
            if (!await roleManager.RoleExistsAsync("User"))
            {
                IdentityRole role = new() { Name = "User"};
                IdentityResult roleResult = await roleManager.CreateAsync(role);
            }
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole role = new() { Name = "Admin" };
                IdentityResult roleResult = await roleManager.CreateAsync(role);
            }

            
            //USERS
            if (await userManager.FindByNameAsync("Admin") is null)
            {
                IdentityUser user = new()
                {
                    UserName = "Admin",
                    Email = "admin@admin.com",
                };
                IdentityResult result = await userManager.CreateAsync(user, "Admin12345.");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user,"Admin");
                }
            }
            if (await userManager.FindByNameAsync("User") is null)
            {
                IdentityUser user = new()
                {
                    UserName = "User",
                    Email = "user@user.com",
                };
                IdentityResult result = await userManager.CreateAsync(user, "User12345.");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }
    }
}
