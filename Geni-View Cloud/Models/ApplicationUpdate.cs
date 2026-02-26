using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GeniView.Cloud.Models
{
    [Index(nameof(AppId), IsUnique = true)]
    public class ApplicationUpdate
    {
        public long ID { get; set; }
        public Guid AppId { get; set; }
        public bool HasUpdate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LatestVersion { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string DownloadAddress { get; set; }

        /// <summary>
        /// Checks if the required fields have proper values.
        /// </summary>
        public bool HasValidUpdateInfo
        {
            get
            {
                try
                {
                    bool success = true;

                    Version v = new Version(LatestVersion);
                    Uri u = new Uri(DownloadAddress);

                    success = ReleaseDate != null;

                    return success;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Used to seed database with application update info.
        /// Don't allow user to add new entries, manage all app IDs here.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApplicationUpdate> Seed()
        {
            return new List<ApplicationUpdate>()
            {
                new ApplicationUpdate()
                {
                    AppId = new Guid("{FF2AA9BE-FDC1-4770-9757-2496EE096A2D}"),
                    Name = "Geni-View Engineer"
                },

                new ApplicationUpdate()
                {
                    AppId = new Guid("{DDE5D450-C955-4D0F-B52E-56727B2C7A64}"),
                    Name = "Geni-View User"
                }
            };
        }
    }
}