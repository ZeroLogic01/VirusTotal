using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirusTotalUI.Models;
using VirusTotalUI.Static;

namespace VirusTotalUI.ViewModels
{
    public class RiskAnalysisSummaryViewModel : BindableBase
    {
        public CloudFishAIAnalysisSummaryViewModel CloudFishAIAnalysisVM { get; set; } = new CloudFishAIAnalysisSummaryViewModel();

        public VirusTotalAnalysisSummaryViewModel VirusTotalAnalysisVM { get; set; } = new VirusTotalAnalysisSummaryViewModel();

        //public async Task<bool> UpdateVirusTotalAnalysisSummary(ObservableCollection<DetailedThreatAnalysisModel> threatAnalysisCollection, CancellationToken cancellationToken)
        //{
        //    if (threatAnalysisCollection?.Count <= 0)
        //    {
        //        return false;
        //    }

        //    await Task.Run(() =>
        //    {
        //        VirusTotalAnalysisVM.TotalEnginesCount = threatAnalysisCollection.Count;
        //        VirusTotalAnalysisVM.TotalMaliciousCount = threatAnalysisCollection.Count(p => p.Category.Equals(ScanCategories.Malicious, StringComparison.OrdinalIgnoreCase));
        //        VirusTotalAnalysisVM.TotalSuspiciousCount = threatAnalysisCollection.Count(p => p.Category.Equals(ScanCategories.Suspicious, StringComparison.OrdinalIgnoreCase));
        //        VirusTotalAnalysisVM.TotalClearCount = threatAnalysisCollection.Count(p => p.Category.Equals(ScanCategories.Undetected, StringComparison.OrdinalIgnoreCase));
        //        VirusTotalAnalysisVM.TotalUnSupportedCount = threatAnalysisCollection.Count(p => p.Category.Equals(ScanCategories.TypeUnsupported, StringComparison.OrdinalIgnoreCase));
        //    }, cancellationToken);
        //    return true;
        //}
    }
}
