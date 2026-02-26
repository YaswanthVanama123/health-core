using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class CycleStatusModel
    {
        // Counts per category
        public int LowCount { get; set; }
        public int HighCount { get; set; }
        public int EndOfLifeCount { get; set; }

        // Displayed under the title
        public int PowerModulesCount { get; set; }

        public int TotalCount { get; set; }

        // Percentages for the stacked bar
        public decimal LowPercent { get; set; }
        public decimal HighPercent { get; set; }
        public decimal EndOfLifePercent { get; set; }

        // Average of the three category counts
        public int AverageCycleCount { get; set; }

        // Battery_ID buckets for popup
        public List<long> LowBatteryIds { get; set; } = new List<long>();
        public List<long> HighBatteryIds { get; set; } = new List<long>();
        public List<long> EndOfLifeBatteryIds { get; set; } = new List<long>();
    }
}