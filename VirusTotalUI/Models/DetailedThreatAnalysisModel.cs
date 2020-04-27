using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.Models
{
    public class DetailedThreatAnalysisModel
    {
        public int ID { get; set; }
        public string EngineName { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }
    }
}
