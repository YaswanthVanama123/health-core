using Microsoft.EntityFrameworkCore;

namespace GeniView.Cloud.Repository
{
    // Minimal DbContext used only to ensure the Hangfire database exists.
    // Hangfire manages its own schema — this context is only used for DB creation.
    public partial class HangfireRepository : DbContext
    {
        public HangfireRepository(DbContextOptions<HangfireRepository> options)
            : base(options)
        {
        }
    }
}
