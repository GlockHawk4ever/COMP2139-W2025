using EventTickets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<AppDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.EnsureCreatedAsync();

        // Roles
        var roles = new[] { "Admin", "Organizer", "Attendee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin user
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Seeded Admin"
            };
            await userManager.CreateAsync(adminUser, "Admin123$");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Organizer user
        var organizerEmail = "organizer@example.com";
        var organizerUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == organizerEmail);
        if (organizerUser is null)
        {
            organizerUser = new ApplicationUser
            {
                UserName = organizerEmail,
                Email = organizerEmail,
                EmailConfirmed = true,
                FullName = "Seeded Organizer"
            };
            await userManager.CreateAsync(organizerUser, "Organizer123$");
            await userManager.AddToRoleAsync(organizerUser, "Organizer");
        }

        // Basic attendee
        var attendeeEmail = "attendee@example.com";
        var attendeeUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == attendeeEmail);
        if (attendeeUser is null)
        {
            attendeeUser = new ApplicationUser
            {
                UserName = attendeeEmail,
                Email = attendeeEmail,
                EmailConfirmed = true,
                FullName = "Seeded Attendee"
            };
            await userManager.CreateAsync(attendeeUser, "Attendee123$");
            await userManager.AddToRoleAsync(attendeeUser, "Attendee");
        }

        // Seed base domain data (categories + events)
        if (!context.Categories.Any())
        {
            AppDbContext.Seed(context, organizerUser);
        }
    }
}
