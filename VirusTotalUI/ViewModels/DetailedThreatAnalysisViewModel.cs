using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using VirusTotalUI.Models;
using VirusTotalUI.Static;
using VTScanner.Results.AnalysisResults;
using Brushes = VirusTotalUI.Static.Brushes;

namespace VirusTotalUI.ViewModels
{
    public class DetailedThreatAnalysisViewModel : BindableBase
    {
        private ObservableCollection<DetailedThreatAnalysisModel> _threatAnalysis;
        public ObservableCollection<DetailedThreatAnalysisModel> ThreatAnalysis
        {
            get { return _threatAnalysis; }
            set { SetProperty(ref _threatAnalysis, value); }
        }

        public static Color GetBackgroundBrushColor(string category)
        {

            if (category.Equals(ScanCategories.Undetected))
            {
                return Static.Colors.Green;
            }

            if (category.Equals(ScanCategories.TypeUnsupported))
            {
                return Static.Colors.Yellow;
            }

            // In case category is ScanCategories.Malicious or Suspicious
            return Static.Colors.Red;
        }

        //public async Task PopulateThreatAnalysisCollection(FileAnalysisResult fileAnalysisResult, CancellationToken cancellationToken)
        //{
        //    ThreatAnalysis = new ObservableCollection<DetailedThreatAnalysisModel>();
        //    await Task.Run(() =>
        //    {
        //        if (fileAnalysisResult != null)
        //        {
        //            int counter = 0;
        //            foreach (KeyValuePair<string, ScanEngine> scan in fileAnalysisResult.Data.Attributes.Results)
        //            {
        //                if (scan.Value.Category.Equals(ScanCategories.ConfirmedTimeout, StringComparison.OrdinalIgnoreCase) ||
        //                     scan.Value.Category.Equals(ScanCategories.Failure, StringComparison.OrdinalIgnoreCase) ||
        //                     scan.Value.Category.Equals(ScanCategories.Harmless, StringComparison.OrdinalIgnoreCase) ||
        //                     scan.Value.Category.Equals(ScanCategories.Timeout, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    continue;
        //                }

        //                // if result is null, choose custom text message or category as description
        //                string description = scan.Value.Result ??
        //                     (scan.Value.Category.Equals(ScanCategories.TypeUnsupported, StringComparison.OrdinalIgnoreCase)
        //                     ? "Unable to process file type" : scan.Value.Category);

        //                ThreatAnalysis.Add(
        //                    new DetailedThreatAnalysisModel
        //                    {
        //                        ID = ++counter,
        //                        EngineName = scan.Key,
        //                        Category = scan.Value.Category,
        //                        Background = GetBackgroundBrush(scan.Value.Category),
        //                        Description = description
        //                    });
        //            }
        //        }
        //    }, cancellationToken).ConfigureAwait(false);

        //}

    }
}
