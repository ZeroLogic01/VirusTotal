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
            if (score < CloudFishAIScore.LowRiskLowerLimit || score > CloudFishAIScore.HighRiskUpperLimit)
            {
                throw new InvalidOperationException($"CloudFish AI score ({score}) is Invalid!");
            }
            else if (score >= CloudFishAIScore.LowRiskLowerLimit & score <= CloudFishAIScore.LowRiskUpperLimit)
            {
                CloudFishAIRating = "Low Risk";
                Foreground = Static.Brushes.Green;
            }
            else if (score > CloudFishAIScore.LowRiskUpperLimit & score <= CloudFishAIScore.MediumRiskUpperLimit)
            {
                CloudFishAIRating = "Medium Risk";
                Foreground = Static.Brushes.Yellow;

            }
            else
            {
                CloudFishAIRating = "High Risk";
                Foreground = Static.Brushes.Red;
            }
        }
    }
}
