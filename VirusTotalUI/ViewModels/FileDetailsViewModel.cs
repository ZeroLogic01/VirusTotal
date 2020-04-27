using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusTotalUI.Models;

namespace VirusTotalUI.ViewModels
{
    public class FileDetailsViewModel : BindableBase
    {
        public FileDetailsModel FileDetails { set; get; } = new FileDetailsModel();
    }
}
