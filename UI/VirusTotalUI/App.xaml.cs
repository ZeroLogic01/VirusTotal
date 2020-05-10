using VirusTotalUI.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Prism.Regions;
using VirusTotalUI.ViewModels;
using Unity.Injection;
using VirusTotalUI.Animations;
using VTScanner;
using VirusTotalUI.Static;

namespace VirusTotalUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static string[] mArgs;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            IRegionManager regionManager = Container.Resolve<IRegionManager>();
            regionManager.Regions[Regions.FileDetailsRegion].Add(Container.Resolve<FileDetailsView>());
            regionManager.Regions[Regions.RiskAnalysisSummaryRegion].Add(Container.Resolve<RiskAnalysisSummaryView>());

            base.OnInitialized();
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindow>();

            containerRegistry.RegisterForNavigation<BeforeDisplayingCloudFishRiskScore>();
            containerRegistry.RegisterForNavigation<WhileCallingVirusTotalAPI>();

        }

        private void VirusTotalUIApplication_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            mArgs = new[] { @"F:\EICAR.txt", "Key.txt", ".51",".2",".6" };
#else
            if (e.Args.Length > 0)
            {
                mArgs = e.Args;
            }
#endif

        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var message = ExceptionHelper.ExtractExceptionMessage(e.Exception);
            MessageBox.Show(message, "An unhandled exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
