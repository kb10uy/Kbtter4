using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Kbtter4.Models;

namespace Kbtter4.ViewModels
{
    public class MainWindowSendingMediaViewModel : ViewModel
    {
        public void Initialize()
        {
        }

        public MainWindowSendingMediaViewModel(string path)
        {
            Path = path;
            MediaStream = File.Open(Path, FileMode.Open);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            MediaStream.Dispose();
        }

        #region Path変更通知プロパティ
        private string _Path;

        public string Path
        {
            get
            { return _Path; }
            set
            { 
                if (_Path == value)
                    return;
                _Path = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region MediaStream変更通知プロパティ
        private Stream _MediaStream;

        public Stream MediaStream
        {
            get
            { return _MediaStream; }
            set
            { 
                if (_MediaStream == value)
                    return;
                _MediaStream = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }
}
