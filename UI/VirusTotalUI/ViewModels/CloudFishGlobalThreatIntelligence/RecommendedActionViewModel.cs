using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using VirusTotalUI.Static;

namespace VirusTotalUI.ViewModels
{
    public class RecommendedActionViewModel : BindableBase
    {
        private string _recommendedAction;
        public string RecommendedAction
        {
            get { return _recommendedAction; }
            set
            {
                SetProperty(ref _recommendedAction, value);
            }
        }

        private Brush _foreground;
        public Brush Foreground
        {
            get { return _foreground; }
            set { SetProperty(ref _foreground, value); }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetProperty(ref _visibility, value); }
        }

        public void SetRecommendedAction(double score)
        {

            if (score < CloudFishAIScore.MinValue || score > CloudFishAIScore.MaxValue)
            {
                throw new InvalidOperationException($"CloudFish AI score ({score}) is Invalid!");
            }
            else if (score >= CloudFishAIScore.MinValue & score < CloudFishAIScore.OptimalRangeStartValue)
            {
                RecommendedAction = "None";
                Foreground = Static.Brushes.GreenBrush;
            }
            else if (score >= CloudFishAIScore.OptimalRangeStartValue & score <= CloudFishAIScore.OptimalRangeEndValue)
            {
                RecommendedAction = "Suspicious";
                Foreground = Static.Brushes.YellowBrush;

            }
            else
            {
                RecommendedAction = "Delete";
                Foreground = Static.Brushes.RedBrush;
            }
        }
        public async Task StartBlinking(CancellationToken cancellationToken, int blinkNumberOfTimes = 25)
        {
            try
            {
                for (int i = 0; i < (blinkNumberOfTimes * 2); i++)
                {
                    Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Visibility = Visibility.Visible;
            }
        }
    }
}
