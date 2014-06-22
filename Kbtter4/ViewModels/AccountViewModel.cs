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
    public class AccountViewModel : ViewModel
    {
        public Kbtter4Account SourceAccount { get; set; }

        public void Initialize()
        {
        }

        public AccountViewModel(Kbtter4Account ac)
        {
            SourceAccount = ac;
            ScreenName = SourceAccount.ScreenName;
            Id = SourceAccount.UserId;
            AccessToken = SourceAccount.AccessToken;
            AccessTokenSecret = SourceAccount.AccessTokenSecret;
        }

        #region ScreenName変更通知プロパティ
        private string _ScreenName = "";

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


        #region Id変更通知プロパティ
        private long _Id = 0;

        public long Id
        {
            get
            { return _Id; }
            set
            {
                if (_Id == value)
                    return;
                _Id = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region AccessToken変更通知プロパティ
        private string _AccessToken = "";

        public string AccessToken
        {
            get
            { return _AccessToken; }
            set
            {
                if (_AccessToken == value)
                    return;
                _AccessToken = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region AccessTokenSecret変更通知プロパティ
        private string _AccessTokenSecret = "";

        public string AccessTokenSecret
        {
            get
            { return _AccessTokenSecret; }
            set
            {
                if (_AccessTokenSecret == value)
                    return;
                _AccessTokenSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
