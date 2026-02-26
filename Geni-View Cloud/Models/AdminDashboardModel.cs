using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class AdminDashboardModel
    {
        public int Communities { get; set; }
        public int Groups { get; set; }
        public int Users { get; set; }
        public int RegDevices { get; set; }
        public int RemDevices { get; set; }
        public int RegBatteries { get; set; }
        public int RemBatteries { get; set; }
    }
}