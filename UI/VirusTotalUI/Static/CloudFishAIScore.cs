using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.Static
{
    public static class CloudFishAIScore
    {
        public static double MinValue { get; set; } = 0d;
        public static double OptimalRangeStartValue { get; set; } = .2d;
        public static double OptimalRangeEndValue { get; set; } = .599999d;
        public static double MaxValue { get; set; } = 1d;
    }
}
