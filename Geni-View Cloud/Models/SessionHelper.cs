using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    // SessionHelper accesses ASP.NET Core session via IHttpContextAccessor.
    // TODO Phase 3: register IHttpContextAccessor in Program.cs:
    //   builder.Services.AddHttpContextAccessor();
    //   builder.Services.AddSession();
    //   app.UseSession();
    // And call SessionHelper.Configure(app.Services.GetRequiredService<IHttpContextAccessor>()) at startup.
    public class SessionHelper
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private static ISession? Session => _httpContextAccessor?.HttpContext?.Session;

        private static void SetSession<T>(string sessionId, T value)
        {
            var session = Session;
            if (session == null) return;
            if (value == null)
            {
                session.Remove(sessionId);
                return;
            }
            string? str = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            session.SetString(sessionId, str ?? string.Empty);
        }

        private static T? GetSession<T>(string sessionId, T? defaultValue)
        {
            var session = Session;
            if (session == null) return defaultValue;
            string? str = session.GetString(sessionId);
            if (str == null) return defaultValue;
            try
            {
                return (T?)Convert.ChangeType(str, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static long? CommunityID
        {
            get => GetSession<long?>("communityID", null);
            set => SetSession<long?>("communityID", value);
        }

        public static long? GroupID
        {
            get => GetSession<long?>("groupID", null);
            set => SetSession<long?>("groupID", value);
        }

        public static bool? IncludeAllSubGroups
        {
            get
            {
                var session = Session;
                if (session == null) return null;
                string? str = session.GetString("includeAllSubGroups");
                if (str == null) return null;
                return str == "True" || str == "true" || str == "1";
            }
            set => SetSession<bool?>("includeAllSubGroups", value);
        }
    }
}
