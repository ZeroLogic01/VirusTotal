using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusTotalUI.ViewModels.RiskAnalysisSummary;

namespace VirusTotalUI.ViewModels
{
    public class RiskAnalysisSummaryViewModel : BindableBase
    {

        public CloudFishAIAnalysisSummaryViewModel CloudFishAIAnalysisVM { get; set; } = new CloudFishAIAnalysisSummaryViewModel();

        public VirusTotalAnalysisSummaryViewModel VirusTotalAnalysisVM { get; set; } = new VirusTotalAnalysisSummaryViewModel();
    
        
    
    }
}
