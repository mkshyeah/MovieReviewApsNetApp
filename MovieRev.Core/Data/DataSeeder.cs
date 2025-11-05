using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieRev.Core.Models;

namespace MovieRev.Core.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        
        string[] roleNames = { Roles.Administrator, Roles.Moderator, Roles.User };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Role '{RoleName}' created.", roleName);
            }
        }

        var adminEmail = configuration["AdminUser:Email"];
        var adminPassword = configuration["AdminUser:Password"];
        
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword)) return;

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = configuration["AdminUser:UserName"],
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    logger.LogError("Error creating admin user: {Error}", error.Description);
                }
                return; 
            }
            logger.LogInformation("Admin user created successfully.");
        }
        else
        {
            // Если пользователь существует, проверим и сбросим пароль, если он не совпадает
            if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
            {
                logger.LogWarning("Admin password mismatch. Resetting password...");
                var removeResult = await userManager.RemovePasswordAsync(adminUser);
                if (!removeResult.Succeeded)
                {
                    logger.LogError("Failed to remove old admin password.");
                    return;
                }
                var addResult = await userManager.AddPasswordAsync(adminUser, adminPassword);
                if (addResult.Succeeded)
                {
                    logger.LogInformation("Admin password has been reset successfully.");
                }
                else
                {
                     logger.LogError("Failed to set new admin password.");
                     return;
                }
            }
        }

        foreach (var roleName in roleNames)
        {
            if (!await userManager.IsInRoleAsync(adminUser, roleName))
            {
                await userManager.AddToRoleAsync(adminUser, roleName);
                logger.LogInformation("Assigned role '{RoleName}' to admin.", roleName);
            }
        }
    }
}