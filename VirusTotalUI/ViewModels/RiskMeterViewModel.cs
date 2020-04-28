using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                DialText = $"Risk {_score}%";
            }
        }

        private string _dialText;

        public string DialText
        {
            get { return _dialText; }
            set
            {
                SetProperty(ref _dialText, value);
            }
        }
    }
}
