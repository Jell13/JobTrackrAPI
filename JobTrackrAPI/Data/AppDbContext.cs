using JobTrackrAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTrackrAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Application> Applications { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<EmployerSponsorshipStat> EmployerSponsorshipStats { get; set; }
    }
}
