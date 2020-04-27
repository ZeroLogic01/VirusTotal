using Prism.Mvvm;

namespace VirusTotalUI.ViewModels.RiskAnalysisSummary
{
    public class VirusTotalAnalysisSummaryViewModel : BindableBase
    {
        private int _totalEnginesCount;
        public int TotalEnginesCount
        {
            get { return _totalEnginesCount; }
            set { SetProperty(ref _totalEnginesCount, value); }
        }

        private int _totalClearCount;
        public int TotalClearCount
        {
            get { return _totalClearCount; }
            set { SetProperty(ref _totalClearCount, value); }
        }
        
        private int _totalMaliciousCount;
        public int TotalMaliciousCount
        {
            get { return _totalMaliciousCount; }
            set { SetProperty(ref _totalMaliciousCount, value); }
        }
        private int _totalSuspiciousCount;
        public int TotalSuspiciousCount
        {
            get { return _totalSuspiciousCount; }
            set { SetProperty(ref _totalSuspiciousCount, value); }
        }

        private int _totalUnSupportedCount;
        public int TotalUnSupportedCount
        {
            get { return _totalUnSupportedCount; }
            set { SetProperty(ref _totalUnSupportedCount, value); }
        }


    }
}
