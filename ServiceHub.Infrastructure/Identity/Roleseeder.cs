using Microsoft.AspNetCore.Identity;

namespace ServiceHub.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        if (roleManager == null)
            throw new ArgumentNullException(nameof(roleManager));

        var roles = new List<string>
        {
            "Admin",
            "Operador"
        };

        foreach (var roleName in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Erro ao criar role '{roleName}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }
}