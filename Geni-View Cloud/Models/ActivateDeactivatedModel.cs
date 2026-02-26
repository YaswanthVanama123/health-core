using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class ActivateDeactivatedModel
    {
        public bool IsChecked { get; set; }
        public string SerialNumber { get; set; }
        public long? BatteryBSN { get; set; }
        public string CommunityName { get; set; }
        public string GroupName { get; set; }
    }
}