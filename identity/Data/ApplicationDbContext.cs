using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Palantir.Identity.Models;

namespace Palantir.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<PalantirUser> PalantirUsers { get; set; }
        public DbSet<PalantirUserRole> PalantirUserRoles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PalantirUserRole>().HasData(new PalantirUserRole { Id = "Administrator", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
            builder.Entity<PalantirUserRole>().HasData(new PalantirUserRole { Id = "User", Name = "User", NormalizedName = "USER" });
            
            base.OnModelCreating(builder);
        }
    }
}
