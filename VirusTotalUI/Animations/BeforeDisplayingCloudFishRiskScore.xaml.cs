using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace VirusTotalUI.Animations
{
    /// <summary>
    /// Interaction logic for BeforeDisplayingCloudFishRiskScore.xaml
    /// </summary>
    public partial class BeforeDisplayingCloudFishRiskScore : UserControl
    {
        public BeforeDisplayingCloudFishRiskScore()
        {
            InitializeComponent();

        }

        /// <summary>
        /// WpfAnimatedGif is decoding Gif on UI thread so in order to avoid 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/Resources/animation-1.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(img, image);
            }));
        }
    }
}
