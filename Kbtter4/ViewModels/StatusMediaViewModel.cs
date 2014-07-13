using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Kbtter4.Models;

namespace Kbtter4.ViewModels
{
    public class StatusMediaViewModel : ViewModel
    {
        public void Initialize()
        {
        }


        #region Uri変更通知プロパティ
        private Uri _Uri;

        public Uri Uri
        {
            get
            { return _Uri; }
            set
            { 
                if (_Uri == value)
                    return;
                _Uri = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
