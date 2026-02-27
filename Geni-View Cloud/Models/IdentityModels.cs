using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeniView.Cloud.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Nullable<long> CommunityID { get; set; }
        public Nullable<long> GroupID { get; set; }
        public string? FullName { get; set; }
        public byte[]? ProfilePhoto { get; set; }
        public string? ImageMimeType { get; set; }
        public string? TimeZoneId { get; set; }
        public bool IsNotificationEnable { get; set; }

        // Navigation property for ASP.NET Core Identity roles via UserRoles join table.
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }
            = new List<IdentityUserRole<string>>();
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Parameterless constructor for use until DI is fully wired in Phase 3.
        public ApplicationDbContext()
            : base(new DbContextOptionsBuilder<ApplicationDbContext>().Options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("GeniViewCloudIdentityRepository"));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Map the Roles navigation property to the IdentityUserRole join table.
            builder.Entity<ApplicationUser>()
                   .HasMany(u => u.Roles)
                   .WithOne()
                   .HasForeignKey(ur => ur.UserId)
                   .IsRequired();
        }
    }
}
