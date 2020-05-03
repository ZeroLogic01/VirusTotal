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
using System.Windows.Input;
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
        #region UI

        #region Private Member

        /// <summary>
        /// The window this view model controls
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// The window resizer helper that keeps the window size correct in various states
        /// </summary>
        private WindowResizer mWindowResizer;

        /// <summary>
        /// The margin around the window to allow for a drop shadow
        /// </summary>
        private Thickness mOuterMarginSize = new Thickness(5);

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        private int mWindowRadius = 10;

        /// <summary>
        /// The last known dock position
        /// </summary>
        private WindowDockPosition mDockPosition = WindowDockPosition.Undocked;

        #endregion

        #region Public Properties

        /// <summary>
        /// The smallest width the window can go to
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 900;

        /// <summary>
        /// The smallest height the window can go to
        /// </summary>
        public double WindowMinimumHeight { get; set; } = 600;

        /// <summary>
        /// True if the window is currently being moved/dragged
        /// </summary>
        public bool BeingMoved { get; set; }


        /// <summary>
        /// True if the window should be borderless because it is docked or maximized
        /// </summary>
        public bool Borderless => (mWindow.WindowState == WindowState.Maximized || mDockPosition != WindowDockPosition.Undocked);

        /// <summary>
        /// The size of the resize border around the window
        /// </summary>
        public int ResizeBorder => mWindow.WindowState == WindowState.Maximized ? 0 : 4;

        /// <summary>
        /// The size of the resize border around the window, taking into account the outer margin
        /// </summary>
        public Thickness ResizeBorderThickness => new Thickness(OuterMarginSize.Left + ResizeBorder,
                                                                OuterMarginSize.Top + ResizeBorder,
                                                                OuterMarginSize.Right + ResizeBorder,
                                                                OuterMarginSize.Bottom + ResizeBorder);

        /// <summary>
        /// The padding of the inner content of the main window
        /// </summary>
        public Thickness InnerContentPadding { get; set; } = new Thickness(0);

        /// <summary>
        /// The margin around the window to allow for a drop shadow
        /// </summary>
        public Thickness OuterMarginSize
        {
            // If it is maximized or docked, no border
            get => mWindow.WindowState == WindowState.Maximized ? mWindowResizer.CurrentMonitorMargin : (Borderless ? new Thickness(0) : mOuterMarginSize);
            set => mOuterMarginSize = value;
        }

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        public int WindowRadius
        {
            // If it is maximized or docked, no border
            get => Borderless ? 0 : mWindowRadius;
            set => mWindowRadius = value;
        }

        /// <summary>
        /// The rectangle border around the window when docked
        /// </summary>
        public int FlatBorderThickness => Borderless && mWindow.WindowState != WindowState.Maximized ? 1 : 0;

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        public CornerRadius WindowCornerRadius => new CornerRadius(WindowRadius);

        /// <summary>
        /// The height of the title bar / caption of the window
        /// </summary>
        public int TitleHeight { get; set; } = 47;
        /// <summary>
        /// The height of the title bar / caption of the window
        /// </summary>
        public GridLength TitleHeightGridLength => new GridLength(TitleHeight + ResizeBorder);

        /// <summary>
        /// True if we should have a dimmed overlay on the window
        /// such as when a popup is visible or the window is not focused
        /// </summary>
        public bool DimmableOverlayVisible { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// The command to minimize the window
        /// </summary>
        public DelegateCommand MinimizeCommand { get; set; }

        /// <summary>
        /// The command to maximize the window
        /// </summary>
        public DelegateCommand MaximizeCommand { get; set; }

        /// <summary>
        /// The command to close the window
        /// </summary>
        public DelegateCommand CloseCommand { get; set; }

        /// <summary>
        /// The command to show the system menu of the window
        /// </summary>
        public DelegateCommand MenuCommand { get; set; }

        #endregion


        #region Private Helpers

        /// <summary>
        /// Gets the current mouse position on the screen
        /// </summary>
        /// <returns></returns>
        private Point GetMousePosition()
        {
            return mWindowResizer.GetCursorPosition();
        }

        /// <summary>
        /// If the window resizes to a special position (docked or maximized)
        /// this will update all required property change events to set the borders and radius values
        /// </summary>
        private void WindowResized()
        {
            // Fire off events for all properties that are affected by a resize
            RaisePropertyChanged(nameof(Borderless));
            RaisePropertyChanged(nameof(FlatBorderThickness));
            RaisePropertyChanged(nameof(ResizeBorderThickness));
            RaisePropertyChanged(nameof(OuterMarginSize));
            RaisePropertyChanged(nameof(WindowRadius));
            RaisePropertyChanged(nameof(WindowCornerRadius));
        }


        #endregion


        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindowViewModel(Window window, IContainerExtension container) : base()
        {
            _container = container;
            _cancellationTokenSource = new CancellationTokenSource();

            mWindow = window;

            // Listen out for the window resizing
            mWindow.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by a resize
                WindowResized();
            };

            // Create commands
            MinimizeCommand = new DelegateCommand(() => mWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new DelegateCommand(() => mWindow.WindowState ^= WindowState.Maximized);
            ClosingCommand = new DelegateCommand(ClosingThreatAnalysis);
            CloseCommand = new DelegateCommand(() => mWindow.Close());
            MenuCommand = new DelegateCommand(() => SystemCommands.ShowSystemMenu(mWindow, GetMousePosition()));
            StartCommand = new DelegateCommand(StartAnalysis);

            // Fix window resize issue
            mWindowResizer = new WindowResizer(mWindow);

            // Listen out for dock changes
            mWindowResizer.WindowDockChanged += (dock) =>
                    {
                        // Store last position
                        mDockPosition = dock;

                        // Fire off resize events
                        WindowResized();
                    };

            // On window being moved/dragged
            mWindowResizer.WindowStartedMove += () =>
                        {
                            // Update being moved flag
                            BeingMoved = true;
                        };

            // Fix dropping an undocked window at top which should be positioned at the
            // very top of screen
            mWindowResizer.WindowFinishedMove += () =>
            {
                // Update being moved flag
                BeingMoved = false;

                // Check for moved to top of window and not at an edge
                if (mDockPosition == WindowDockPosition.Undocked && mWindow.Top == mWindowResizer.CurrentScreenSize.Top)
                    // If so, move it to the true top (the border size)
                    mWindow.Top = -OuterMarginSize.Top;
            };
        }

        #endregion

        #region Business Logic

        #region Private Fields
        private readonly IContainerProvider _container;
        private CancellationTokenSource _cancellationTokenSource;
        private Scanner _scanner;
        #endregion

        #region Public Properties

        private string _title = "CloudFish Global Threat Intelligence";
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

        private RiskMeterViewModel _riskMeterVM = new RiskMeterViewModel();
        public RiskMeterViewModel RiskMeterVM
        {
            get { return _riskMeterVM; }
            set
            {
                SetProperty(ref _riskMeterVM, value);
            }
        }

        public DelegateCommand StartCommand { private set; get; }
        public DelegateCommand ClosingCommand { private set; get; }


        #endregion


        #region Event Handlers
        private void Scanner_OnProgressChanged(string obj)
        {
            Console.WriteLine(obj);
        }
        #endregion

        #region Private Methods

        private async void StartAnalysis()
        {
            try
            {
                string[] args = App.mArgs;
                string fileToScan = args[0];
                string apiKeyFile = args[1];
                var cloudFishScore = float.Parse(args[2]);
                //CloudFishAIScore.LowRiskUpperLimit = float.Parse(args[3]);
                //CloudFishAIScore.MediumRiskUpperLimit = float.Parse(args[3]);

                if (!File.Exists(fileToScan))
                {
                    throw new FileNotFoundException($"Could not find the file '{fileToScan}'.");
                }

                System.IO.FileInfo file = new System.IO.FileInfo(fileToScan);
                await InitializeFileDetails(file).ConfigureAwait(false);
                await ShowFirstAnimationBeforeDisplayingCloudFishScore().ConfigureAwait(false);
                DisplayCloudFishAIRiskAnalysisSummary(cloudFishScore);
                AddViewToRegion(Regions.AnalysisProgressRegion.ToString(), typeof(WhileCallingVirusTotalAPI));
                //await Task.Delay(1500).ConfigureAwait(false);
                await ScanFile(apiKeyFile, fileToScan).ConfigureAwait(false);
                RemoveViewFromRegion(Regions.AnalysisProgressRegion.ToString(), typeof(WhileCallingVirusTotalAPI));

                AddViewToRegion(Regions.RecommendedActionRegion.ToString(), typeof(RecommendedActionView));
                CloudFishGlobalThreatIntelligenceVM.RecommendedActionVM.SetRecommendedAction(cloudFishScore);
                AddViewToRegion(Regions.AnalysisProgressRegion.ToString(), typeof(RiskMeterView));
                await Task.Delay(1000).ConfigureAwait(false);
                RiskMeterVM.Score = cloudFishScore;
                RiskMeterVM.CalculateRiskPercentage(cloudFishScore);
                CloudFishGlobalThreatIntelligenceVM.RecommendedActionVM.StartBlinking(_cancellationTokenSource.Token).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                RemoveViewFromRegion(Regions.AnalysisProgressRegion, typeof(BeforeDisplayingCloudFishRiskScore));
                RemoveViewFromRegion(Regions.AnalysisProgressRegion, typeof(WhileCallingVirusTotalAPI));

                string exceptionMessage = ExceptionHelper.ExtractExceptionMessage(ex);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"Operation canceled: {exceptionMessage}");
                }
                else
                {
                    MessageBox.Show(exceptionMessage.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


            }
            finally
            {
                if (_scanner != null)
                {
                    _scanner.Dispose();
                    _scanner.OnProgressChanged -= Scanner_OnProgressChanged;
                }
            }
        }

        private async Task ScanFile(string apiKeyFile, string fileToScan)
        {
            _scanner = new Scanner(apiKeyFile, _cancellationTokenSource.Token);
            _scanner.OnProgressChanged += Scanner_OnProgressChanged;
            string fileHash = CloudFishGlobalThreatIntelligenceVM.FileDetailsVM.FileDetails.SHA256;

            FileAnalysisResult fileAnalysisResult = await _scanner.GetFileAnalysisResultAsync(new System.IO.FileInfo(fileToScan), fileHash);

            RemoveViewFromRegion(Regions.AnalysisProgressRegion.ToString(), typeof(WhileCallingVirusTotalAPI));

            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
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

                        var color = DetailedThreatAnalysisViewModel.GetBackgroundBrushColor(scan.Value.Category);
                        var flashing = scan.Value.Category.Equals(ScanCategories.Suspicious, StringComparison.OrdinalIgnoreCase) ||
                             scan.Value.Category.Equals(ScanCategories.Malicious, StringComparison.OrdinalIgnoreCase) ? true : false;

                        reports.Add(
                            new DetailedThreatAnalysisModel
                            {
                                ID = ++counter,
                                EngineName = scan.Key,
                                Category = scan.Value.Category,
                                IsFlashing = flashing,
                                Background = color,
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
            }, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        private async Task ShowFirstAnimationBeforeDisplayingCloudFishScore()
        {
            AddViewToRegion(Regions.AnalysisProgressRegion.ToString(), typeof(BeforeDisplayingCloudFishRiskScore));
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            RemoveViewFromRegion(Regions.AnalysisProgressRegion.ToString(), typeof(BeforeDisplayingCloudFishRiskScore));
        }

        private void DisplayCloudFishAIRiskAnalysisSummary(float score)
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                CloudFishGlobalThreatIntelligenceVM.RiskAnalysisSummaryVM.CloudFishAIAnalysisVM.SetRating(score);
                AddViewToRegion(Regions.CloudFishAIAnalysisSummaryRegion, typeof(CloudFishAIAnalysisSummaryView));
            }));
        }

        private void AddViewToRegion(string regionName, Type T)
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                IRegion region = _container.Resolve<IRegionManager>().Regions[regionName];
                var view = _container.Resolve(T.UnderlyingSystemType);
                region.Add(view);
            }));
        }

        private void RemoveViewFromRegion(string regionName, Type T)
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                IRegion region = _container.Resolve<IRegionManager>().Regions[regionName];
                object view = region.Views.SingleOrDefault(v => v.GetType().Name == T.Name);

                if (view != null)
                {
                    region.Deactivate(view);
                }
            }), System.Windows.Threading.DispatcherPriority.Normal);
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
            }, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        private void ClosingThreatAnalysis()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExceptionHelper.ExtractExceptionMessage(ex));
            }

            Application.Current?.Shutdown();
        }


        #endregion

        #endregion
    }
}
