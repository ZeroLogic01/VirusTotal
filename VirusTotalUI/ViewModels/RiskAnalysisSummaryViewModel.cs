using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.ViewModels
{
    public class RiskAnalysisSummaryViewModel : BindableBase
    {
        private string _cloudFishAIRating = "High Risk";
        public string CloudFishAIRating
        {
            get { return _cloudFishAIRating; }
            set { SetProperty(ref _cloudFishAIRating, value); }
        }


        private int _totalEnginesCount = 72;
        public int TotalEnginesCount
        {
            get { return _totalEnginesCount; }
            set { SetProperty(ref _totalEnginesCount, value); }
        }

        private int _totalClearCount = 32;
        public int TotalClearCount
        {
            get { return _totalClearCount; }
            set { SetProperty(ref _totalClearCount, value); }
        }

        private int _totalMaliciousCount = 28;
        public int TotalMaliciousCount
        {
            get { return _totalMaliciousCount; }
            set { SetProperty(ref _totalMaliciousCount, value); }
        }

        private int _totalUnSupportedCount = 12;
        public int TotalUnSupportedCount
        {
            get { return _totalUnSupportedCount; }
            set { SetProperty(ref _totalUnSupportedCount, value); }
        }

        
    }
}
