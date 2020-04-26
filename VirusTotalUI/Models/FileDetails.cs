using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusTotalUI.Models
{
    public class FileDetails : BindableBase
    {
        private string _path = "hello";

        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        private string _fileName;

        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        private string _mD5;

        public string MD5
        {
            get { return _mD5; }
            set { SetProperty(ref _mD5, value); }
        }

        private string _sHA1;

        public string SHA1
        {
            get { return _sHA1; }
            set { SetProperty(ref _sHA1, value); }
        }

        private string _sHA12;

        public string SHA12
        {
            get { return _sHA12; }
            set { SetProperty(ref _sHA12, value); }
        }

        private string _shA256;

        public string SHA256
        {
            get { return _shA256; }
            set { SetProperty(ref _shA256, value); }
        }
    }
}
