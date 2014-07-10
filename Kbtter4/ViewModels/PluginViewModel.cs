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
using Kbtter4.Models.Plugin;

namespace Kbtter4.ViewModels
{
    public class PluginViewModel : ViewModel
    {
        public PluginViewModel(Kbtter4Plugin p)
        {
            Name = p.Name;
        }

        public void Initialize()
        {
        }


        #region Name変更通知プロパティ
        private string _Name;

        public string Name
        {
            get
            { return _Name; }
            set
            { 
                if (_Name == value)
                    return;
                _Name = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
