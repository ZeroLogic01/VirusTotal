using Prism.Mvvm;
using System;
using System.Windows.Media;
using VirusTotalUI.Static;

namespace VirusTotalUI.ViewModels
{
    public class CloudFishAIAnalysisSummaryViewModel : BindableBase
    {
        private string _cloudFishAIRating;
        public string CloudFishAIRating
        {
            get { return _cloudFishAIRating; }
            set { SetProperty(ref _cloudFishAIRating, value); }
        }

        private Brush _ratingFieldForegroundColor;
        public Brush Foreground
        {
            get { return _ratingFieldForegroundColor; }
            set { SetProperty(ref _ratingFieldForegroundColor, value); }
        }

        public void SetRating(float score)
        {
            if (score < CloudFishAIScore.MinValue || score > CloudFishAIScore.MaxValue)
            {
                throw new InvalidOperationException($"CloudFish AI score ({score}) is Invalid!");
            }
            else if (score >= CloudFishAIScore.MinValue & score < CloudFishAIScore.OptimalRangeStartValue)
            {
                CloudFishAIRating = "Low Risk";
                Foreground = Static.Brushes.GreenBrush;
            }
            else if (score >= CloudFishAIScore.OptimalRangeStartValue & score <= CloudFishAIScore.OptimalRangeEndValue)
            {
                CloudFishAIRating = "Medium Risk";
                Foreground = Static.Brushes.YellowBrush;

            }
            else
            {
                CloudFishAIRating = "High Risk";
                Foreground = Static.Brushes.RedBrush;
            }
        }
    }
}
