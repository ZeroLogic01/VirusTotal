using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using VirusTotalUI.Animations;
using VTScanner;
using VTScanner.Helpers;

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

        private Scanner scanner;

        public MainWindowViewModel(IContainerExtension container) : base()
        {
            _container = container;
            StartCommand = new DelegateCommand(StartAnalysis);
        }

        private async void StartAnalysis()
        {
            try
            {
                string[] args = App.mArgs;
                string fileToScan = args[0];
                string apiKeyFile = args[1];
                int cloudFishScore = int.Parse(args[2]);

                FileInfo file = new FileInfo(fileToScan);
                await ShowFirstAnimationBeforeDisplayingCloudFishScore();
                await InitializeFileDetails(file).ConfigureAwait(false);
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

        private void Scanner_OnProgressMade(string obj)
        {
            throw new NotImplementedException();
        }

        #region Methods

        private async Task ShowFirstAnimationBeforeDisplayingCloudFishScore()
        {
            try
            {
                IRegionManager regionManager = _container.Resolve<IRegionManager>();
                IRegion region = regionManager.Regions["AnalysisProgressRegion"];
                var view = _container.Resolve<BeforeDisplayingCloudFishRiskScore>();
                region.Add(view);
                await Task.Delay(TimeSpan.FromSeconds(3));
                region.Remove(view);
            }
            catch (Exception)
            {

            }
        }

        private async Task InitializeFileDetails(FileInfo file)
        {
            await Task.Run(() =>
            {
                FileDetailsVM.Path = file.DirectoryName;
                FileDetailsVM.FileName = file.Name;


                FileDetailsVM.MD5 = "Calculating...";
                FileDetailsVM.MD5 = HashHelper.GetMd5(file);
                FileDetailsVM.SHA1 = "Calculating...";

                FileDetailsVM.SHA1 = HashHelper.GetSha1(file);
                FileDetailsVM.SHA256 = "Calculating...";

                FileDetailsVM.SHA256 = HashHelper.GetSha256(file);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
