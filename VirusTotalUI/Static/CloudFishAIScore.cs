using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.Static
{
    public static class CloudFishAIScore
    {
        public static float LowRiskLowerLimit { get; set; } = 0f;
        public static float LowRiskUpperLimit { get; set; } = .2f;
        public static float MediumRiskUpperLimit { get; set; } = .599999f;
        public static float HighRiskUpperLimit { get; set; } = 1f;
    }
}
