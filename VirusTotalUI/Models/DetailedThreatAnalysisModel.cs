using System.Windows.Media;

namespace VirusTotalUI.Models
{
    public class DetailedThreatAnalysisModel
    {
        public int ID { get; set; }
        public string EngineName { get; set; }

        public string Category { get; set; }

        public Brush Background { get; set; }

        public string Description { get; set; }
    }
}
