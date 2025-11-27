using Microsoft.EntityFrameworkCore;
using COMP2139_ICE.Models;

namespace COMP2139_ICE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
    }
}
