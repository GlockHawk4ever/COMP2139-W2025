using EventTickets.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

    public static void Seed(AppDbContext db, ApplicationUser? organizerUser = null)
    {
        if (db.Categories.Any())
        {
            return;
        }

        var webinar = new Category { Name = "Webinar", Description = "Online session" };
        var concert = new Category { Name = "Concert", Description = "Live music" };
        var workshop = new Category { Name = "Workshop" };

        db.Categories.AddRange(webinar, concert, workshop);
        db.SaveChanges();

        var events = new[]
        {
            new Event
            {
                Title = "Sample Webinar",
                CategoryId = webinar.Id,
                DateTime = DateTime.Now.AddDays(7),
                Price = 0,
                AvailableTickets = 100
            },
            new Event
            {
                Title = "Campus Concert",
                CategoryId = concert.Id,
                DateTime = DateTime.Now.AddDays(14),
                Price = 25,
                AvailableTickets = 50
            },
            new Event
            {
                Title = "C# Workshop",
                CategoryId = workshop.Id,
                DateTime = DateTime.Now.AddDays(3),
                Price = 10,
                AvailableTickets = 0 // Sold-out example
            }
        };

        if (organizerUser is not null)
        {
            foreach (var ev in events)
            {
                ev.OrganizerId = organizerUser.Id;
            }
        }

        db.Events.AddRange(events);
        db.SaveChanges();
    }
}
