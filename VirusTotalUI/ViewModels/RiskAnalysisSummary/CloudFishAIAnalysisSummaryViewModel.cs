using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VirusTotalUI.ViewModels.RiskAnalysisSummary
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
            if (score < 0 || score > 1)
            {
                throw new InvalidOperationException($"CloudFish AI score ({score}) is Invalid!");
            }
            else if (score >= 0 & score <= .2)
            {
                CloudFishAIRating = "Low Risk";
                Foreground = Static.Colors.Green;
            }
            else if (score > .2 & score < .6)
            {
                CloudFishAIRating = "Medium Risk";
                Foreground = Static.Colors.Yellow;

            }
            else
            {
                CloudFishAIRating = "High Risk";
                Foreground = Static.Colors.Red;
            }
        }
    }
}
