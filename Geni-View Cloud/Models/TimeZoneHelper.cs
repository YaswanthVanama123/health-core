using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class TimeZoneHelper
    {
        // NOTE: The overloads that resolved the current user from HttpContext.Current are removed.
        // In ASP.NET Core, the current user must be passed explicitly (use the overloads below)
        // or resolved via IHttpContextAccessor + UserManager in the calling controller.

        public static string GetLocalDateTime(DateTime dateTime, ApplicationUser currentUser)
        {
            if (currentUser == null || string.IsNullOrEmpty(currentUser.TimeZoneId))
                return dateTime.ToString(Global.dateTimeFormat);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId!)).ToString(Global.dateTimeFormat);
        }

        public static string GetLocalDate(DateTime dateTime, ApplicationUser currentUser)
        {
            if (currentUser == null || string.IsNullOrEmpty(currentUser.TimeZoneId))
                return dateTime.ToShortDateString();
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId!)).ToShortDateString();
        }

        public static DateTime ConvertToUTC(DateTime dateTime, ApplicationUser currentUser)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId!));
        }

        public static List<TimeZoneInfoHelper> GetTimeZoneList()
        {
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            var timeZoneList = from b in tz
                               select new TimeZoneInfoHelper
                               {
                                   ID = b.Id,
                                   DisplayText = b.DisplayName
                               };
            return timeZoneList.ToList();
        }

        public class TimeZoneInfoHelper
        {
            public string ID { get; set; } = string.Empty;
            public string DisplayText { get; set; } = string.Empty;
        }
    }
}
