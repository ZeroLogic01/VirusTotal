using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusTotalUI.Static;

namespace VirusTotalUI.ViewModels
{
    public class RiskMeterViewModel : BindableBase
    {
        private double _score = 0;

        public double Score
        {
            get { return _score; }
            set
            {
                SetProperty(ref _score, value);
            }
        }

        private string _dialText = string.Empty;

        public string DialText
        {
            get { return _dialText; }
            set
            {
                SetProperty(ref _dialText, value);
            }
        }

        public void CalculateRiskPercentage(double score)
        {
            var risk = score * 100 / CloudFishAIScore.MaxValue;
            DialText = $"Risk {Math.Round(risk)}%";
        }
    }
}
