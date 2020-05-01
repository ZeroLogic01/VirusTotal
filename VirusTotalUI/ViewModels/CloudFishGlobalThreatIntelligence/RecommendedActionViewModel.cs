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

        public void SetRecommendedAction(float score)
        {

            if (score < CloudFishAIScore.LowRiskLowerLimit || score > CloudFishAIScore.HighRiskUpperLimit)
            {
                throw new InvalidOperationException($"CloudFish AI score ({score}) is Invalid!");
            }
            else if (score >= CloudFishAIScore.LowRiskLowerLimit & score <= CloudFishAIScore.LowRiskUpperLimit)
            {
                RecommendedAction = "None";
                Foreground = Static.Brushes.Green;
            }
            else if (score > CloudFishAIScore.LowRiskUpperLimit & score <= CloudFishAIScore.MediumRiskUpperLimit)
            {
                RecommendedAction = "Suspicious";
                Foreground = Static.Brushes.Yellow;

            }
            else
            {
                RecommendedAction = "Delete";
                Foreground = Static.Brushes.Red;
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
