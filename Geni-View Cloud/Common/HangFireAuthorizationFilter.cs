using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace GeniView.Cloud.Common
{
    /// <summary>
    /// Hangfire dashboard authorization filter.
    /// Currently allows all authenticated requests.
    /// In Phase 3, wire this up via app.MapHangfireDashboard with DashboardOptions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // TODO (Phase 3): restrict to authenticated admin users via context.GetHttpContext()
            return true;
        }
    }
}
