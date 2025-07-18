
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Models;

namespace OluBackendApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ArtisanProfile> ArtisanProfiles { get; set; }
        public DbSet<OfficeOwnerProfile> OfficeOwnerProfiles { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }
        public DbSet<SuperAdminProfile> SuperAdminProfiles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ArtisanProfile>()
                .HasKey(p => p.UserId);
            builder.Entity<ArtisanProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.ArtisanProfile)
                .HasForeignKey<ArtisanProfile>(p => p.UserId);

            builder.Entity<OfficeOwnerProfile>()
                .HasKey(p => p.UserId);
            builder.Entity<OfficeOwnerProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.OfficeOwnerProfile)
                .HasForeignKey<OfficeOwnerProfile>(p => p.UserId);

            builder.Entity<AdminProfile>()
           .HasOne(p => p.User)
           .WithOne()
           .HasForeignKey<AdminProfile>(p => p.UserId);

            builder.Entity<SuperAdminProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<SuperAdminProfile>(p => p.UserId);
        }
    }
}
