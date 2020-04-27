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
            regionManager.Regions[Regions.RiskAnalysisSummaryRegion].Add(Container.Resolve<RiskAnalysisSummaryView>());
            regionManager.Regions[Regions.FileDetailsRegion].Add(Container.Resolve<FileDetailsView>());

            //

            base.OnInitialized();
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<BeforeDisplayingCloudFishRiskScore>();
            containerRegistry.RegisterForNavigation<WhileCallingVirusTotalAPI>();

        }

        private void VirusTotalUIApplication_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            mArgs = new[] { @"F:\Videos\top 4.mp4", "Key.txt", "1" };
#else
            if (e.Args.Length > 0)
            {
                mArgs = e.Args;
            }
#endif

        }
    }
}
