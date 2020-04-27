using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Unity;
using VirusTotalUI.Animations;
using VirusTotalUI.Models;
using VirusTotalUI.Static;
using VirusTotalUI.Views.RiskAnalysisSummary;
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

        public DelegateCommand StartCommand { private set; get; }


        //public MainWindowViewModel()
        //{
        //}

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
            }
        }

        private async Task ScanFile(string apiKeyFile, string fileToScan)
        {
            _scanner = new Scanner(apiKeyFile, _cancellationToken);

            var fileAnalysisResult = await _scanner.GetFileAnalysisResultAsync(new System.IO.FileInfo(fileToScan), FileDetailsVM.FileDetails.SHA12);

            List<DetailedThreatAnalysisModel> reports = new List<DetailedThreatAnalysisModel>();
            if (fileAnalysisResult != null)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileAnalysisResult.Data.Attributes.Results)
                {
                    if (scan.Value.Category.Equals("confirmed-timeout") ||
                         scan.Value.Category.Equals("failure") ||
                         scan.Value.Category.Equals("harmless"))
                    {
                        continue;
                    }

                    // if result is null, choose custom text message or category as description
                    string description = scan.Value.Result ??
                        (scan.Value.Category.Equals(@"type-unsupported", StringComparison.OrdinalIgnoreCase)
                        ? "Unable to process file type" : scan.Value.Category);

                    reports.Add(
                        new DetailedThreatAnalysisModel
                        {
                            EngineName = scan.Key,
                            Category = scan.Value.Category,
                            Description = description
                        });
                }
            }

            if (reports.Count > 0)
            {
                RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalEnginesCount = reports.Count;
                RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalMaliciousCount = reports.Count(p => p.Category.Equals("malicious"));
                RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalSuspiciousCount = reports.Count(p => p.Category.Equals("suspicious"));
                RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalClearCount = reports.Count(p => p.Category.Equals("undetected"));
                RiskAnalysisSummaryVM.VirusTotalAnalysisVM.TotalUnSupportedCount = reports.Count(p => p.Category.Equals("type-unsupported"));

                AddViewToRegion(Regions.VirusTotalAnalysisSummaryRegion.ToString(), typeof(VirusTotalAnalysisSummaryView));

            }

        }

        private void Scanner_OnProgressMade(string obj)
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
                RiskAnalysisSummaryVM.CloudFishAIAnalysisVM.SetRating((float).1);
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
                FileDetailsVM.FileDetails.Path = file.DirectoryName;
                FileDetailsVM.FileDetails.FileName = file.Name;

                FileDetailsVM.FileDetails.MD5 = "Calculating...";
                FileDetailsVM.FileDetails.MD5 = HashHelper.GetMd5(file);

                FileDetailsVM.FileDetails.SHA1 = "Calculating...";
                FileDetailsVM.FileDetails.SHA1 = HashHelper.GetSha1(file);

                FileDetailsVM.FileDetails.SHA256 = "Calculating...";
                FileDetailsVM.FileDetails.SHA256 = HashHelper.GetSha256(file);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
