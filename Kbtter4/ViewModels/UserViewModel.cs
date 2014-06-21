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
    public class UserViewModel : ViewModel
    {
        Kbtter4User src;

        public UserViewModel(Kbtter4User user)
        {
            src = user;
        }

        public void Initialize()
        {
        }


        #region Name変更通知プロパティ
        private string _Name = "Name";

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


        #region ScreenName変更通知プロパティ
        private string _ScreenName="ScreenName";

        public string ScreenName
        {
            get
            { return _ScreenName; }
            set
            {
                if (_ScreenName == value)
                    return;
                _ScreenName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
