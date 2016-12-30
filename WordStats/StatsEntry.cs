using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordStats
{
    public class StatsEntry
    {
        public string Word { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }

        public string DisplayedCountWithPercentage
        {
            get
            {
                return Count + " (" + Percentage.ToString("0.00") + "%)";
            }
        }
    }
}
