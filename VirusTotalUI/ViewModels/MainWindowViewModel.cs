using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using VirusTotalUI.Animations;
using VirusTotalUI.Models;
using VirusTotalUI.Static;
using VirusTotalUI.Views;
using VTScanner;
using VTScanner.Helpers;
using VTScanner.Results.AnalysisResults;

namespace VirusTotalUI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IContainerProvider _container;
        private CancellationToken _cancellationToken;

        private string _title = "";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private CloudFishGlobalThreatIntelligenceViewModel _cloudFishGlobalThreatIntelligenceVM = new CloudFishGlobalThreatIntelligenceViewModel();
        public CloudFishGlobalThreatIntelligenceViewModel CloudFishGlobalThreatIntelligenceVM
        {
            get { return _cloudFishGlobalThreatIntelligenceVM; }
            set
            {
                SetProperty(ref _cloudFishGlobalThreatIntelligenceVM, value);
            }
        }

        private DetailedThreatAnalysisViewModel _detailedThreatAnalysisVM = new DetailedThreatAnalysisViewModel();
        public DetailedThreatAnalysisViewModel DetailedThreatAnalysisVM
        {
            get { return _detailedThreatAnalysisVM; }
            set
            {
                SetProperty(ref _detailedThreatAnalysisVM, value);
            }
        }

        public DelegateCommand StartCommand { private set; get; }



        private Scanner _scanner;

        public MainWindowViewModel(IContainerExtension container) : base()
        {
            _container = container;
            _cancellationToken = new CancellationToken();
            StartCommand = new DelegateCommand(StartAnalysis);
        }

        private async void StartAnalysis()
        {
            try
            {
                string[] args = App.mArgs;
                string fileToScan = args[0];
                string apiKeyFile = args[1];
                var cloudFishScore = float.Parse(args[2]);

                if (!File.Exists(fileToScan))
                {
                    throw new FileNotFoundException($"Could not find the file '{fileToScan}'.");
                }

                System.IO.FileInfo file = new System.IO.FileInfo(fileToScan);
                await InitializeFileDetails(file).ConfigureAwait(false);
                await ShowFirstAnimationBeforeDisplayingCloudFishScore().ConfigureAwait(false);
                DisplayCloudFishAIRiskAnalysisSummary(cloudFishScore);
                AddViewToRegion(Regions.AnalysisProgressRegion.ToString(), typeof(WhileCallingVirusTotalAPI));

                await ScanFile(apiKeyFile, fileToScan).ConfigureAwait(false);
                RemoveViewFromRegion(Regions.AnalysisProgressRegion.ToString(), typeof(WhileCallingVirusTotalAPI));

            }
            catch (Exception ex)
            {
                StringBuilder exceptionText = new StringBuilder();
                exceptionText.Append(ex.Message);
                while (ex.InnerException != null)
                {
                    exceptionText.Append($" {ex.InnerException.Message}");
                    ex = ex.InnerException;
                }
                Console.Error.WriteLine(exceptionText.ToString());
                if (_scanner != null)
                {
                    _scanner.OnProgressChanged -= Scanner_OnProgressChanged;
                }
            }
        }

        private async Task ScanFile(string apiKeyFile, string fileToScan)
        {
            _scanner = new Scanner(apiKeyFile, _cancellationToken);
            _scanner.OnProgressChanged += Scanner_OnProgressChanged;
            string fileHash = CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA256;

            FileAnalysisResult fileAnalysisResult = await _scanner.GetFileAnalysisResultAsync(new System.IO.FileInfo(fileToScan), fileHash);

            _cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                var reports = new ObservableCollection<DetailedThreatAnalysisModel>();
                if (fileAnalysisResult != null)
                {
                    int counter = 0;
                    foreach (KeyValuePair<string, ScanEngine> scan in fileAnalysisResult.Data.Attributes.Results)
                    {
                        if (scan.Value.Category.Equals(ScanCategories.ConfirmedTimeout, StringComparison.OrdinalIgnoreCase) ||
                             scan.Value.Category.Equals(ScanCategories.Failure, StringComparison.OrdinalIgnoreCase) ||
                             scan.Value.Category.Equals(ScanCategories.Harmless, StringComparison.OrdinalIgnoreCase) ||
                             scan.Value.Category.Equals(ScanCategories.Timeout, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // if result is null, choose custom text message or category as description
                        string description = scan.Value.Result ??
                            (scan.Value.Category.Equals(ScanCategories.TypeUnsupported, StringComparison.OrdinalIgnoreCase)
                            ? "Unable to process file type" : scan.Value.Category);

                        reports.Add(
                            new DetailedThreatAnalysisModel
                            {
                                ID = ++counter,
                                EngineName = scan.Key,
                                Category = scan.Value.Category,
                                Background = DetailedThreatAnalysisViewModel.GetBackgroundBrush(scan.Value.Category),
                                Description = description
                            });
                    }
                }

                if (reports.Count > 0)
                {
                    CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalEnginesCount = reports.Count;
                    CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalMaliciousCount = reports.Count(p => p.Category.Equals(ScanCategories.Malicious, StringComparison.OrdinalIgnoreCase));
                    CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalSuspiciousCount = reports.Count(p => p.Category.Equals(ScanCategories.Suspicious, StringComparison.OrdinalIgnoreCase));
                    CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalClearCount = reports.Count(p => p.Category.Equals(ScanCategories.Undetected, StringComparison.OrdinalIgnoreCase));
                    CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalUnSupportedCount = reports.Count(p => p.Category.Equals(ScanCategories.TypeUnsupported, StringComparison.OrdinalIgnoreCase));

                    AddViewToRegion(Regions.VirusTotalAnalysisSummaryRegion.ToString(), typeof(VirusTotalAnalysisSummaryView));
                    DetailedThreatAnalysisVM.ThreatAnalysis = reports;
                }
            }, _cancellationToken).ConfigureAwait(false);
        }

        private void Scanner_OnProgressChanged(string obj)
        {
            Console.WriteLine(obj);
        }

        #region Methods

        private async Task ShowFirstAnimationBeforeDisplayingCloudFishScore()
        {
            AddViewToRegion(Regions.AnalysisProgressRegion.ToString(), typeof(BeforeDisplayingCloudFishRiskScore));
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            RemoveViewFromRegion(Regions.AnalysisProgressRegion.ToString(), typeof(BeforeDisplayingCloudFishRiskScore));
        }

        private void DisplayCloudFishAIRiskAnalysisSummary(float score)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.CloudFishAIAnalysisVM.SetRating(score);
                AddViewToRegion(Regions.CloudFishAIAnalysisSummaryRegion, typeof(CloudFishAIAnalysisSummaryView));
            }));
        }

        private void AddViewToRegion(string regionName, Type T)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IRegion region = _container.Resolve<IRegionManager>().Regions[regionName];
                var view = _container.Resolve(T.UnderlyingSystemType);
                region.Add(view);
            }));
        }

        private void RemoveViewFromRegion(string regionName, Type T)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IRegion region = _container.Resolve<IRegionManager>().Regions[regionName];
                object view = region.Views.SingleOrDefault(v => v.GetType().Name == T.Name);

                var views = region.Views.Where(p => ((IRegionMemberLifetime)p).KeepAlive == true);

                if (view != null)
                {
                    region.Deactivate(view);
                }
            }));
        }

        private async Task InitializeFileDetails(System.IO.FileInfo file)
        {
            await Task.Run(() =>
            {
                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.Path = file.DirectoryName;
                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.FileName = file.Name;

                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.MD5 = "Calculating...";
                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.MD5 = HashHelper.GetMd5(file);

                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA1 = "Calculating...";
                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA1 = HashHelper.GetSha1(file);

                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA256 = "Calculating...";
                CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA256 = HashHelper.GetSha256(file);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
