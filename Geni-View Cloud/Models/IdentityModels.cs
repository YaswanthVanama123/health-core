using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

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
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
