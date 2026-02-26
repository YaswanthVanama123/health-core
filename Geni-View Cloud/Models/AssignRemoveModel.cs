using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class AssignRemoveModel
    {
        public bool IsChecked { get; set; }
        public string SerialNumber { get; set; }
        public long? BatteryBSN { get; set; }
        public string CommunityName { get; set; }
    }

    public class AssignRemoveListModel
    {
        public long CommunityID{ get; set; }
        public List<AssignRemoveModel> DeviceList { get; set; }
    }


}