using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VirusTotalUI.ViewModels
{
    public class CloudFishGlobalThreatIntelligenceViewModel : BindableBase
    {
        private FileDetailsViewModel _fileDetailsVM = new FileDetailsViewModel();
        public FileDetailsViewModel FileDetailsVM
        {
            get { return _fileDetailsVM; }
            set
            {
                SetProperty(ref _fileDetailsVM, value);
            }
        }

        private RiskAnalysisSummaryViewModel _riskAnalysisSummaryVM = new RiskAnalysisSummaryViewModel();
        public RiskAnalysisSummaryViewModel RiskAnalysisSummaryVM
        {
            get { return _riskAnalysisSummaryVM; }
            set
            {
                SetProperty(ref _riskAnalysisSummaryVM, value);
            }
        }

        private RecommendedActionViewModel _recommendedActionVM = new RecommendedActionViewModel();
        public RecommendedActionViewModel RecommendedActionVM
        {
            get { return _recommendedActionVM; }
            set
            {
                SetProperty(ref _recommendedActionVM, value);
            }
        }
    }
}
