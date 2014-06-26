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
using CoreTweet;

namespace Kbtter4.ViewModels
{
    public class UserViewModel : ViewModel
    {
        User src;
        PropertyChangedEventListener listener;

        public UserViewModel(User user)
        {
            src = user;
            Name = src.Name;
            ScreenName = src.ScreenName;
            IdString = src.Id.ToString();
            ProfileImageUri = src.ProfileImageUrlHttps;
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
        private string _ScreenName = "ScreenName";

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


        #region IdString変更通知プロパティ
        private string _IdString = "";

        public string IdString
        {
            get
            { return _IdString; }
            set
            {
                if (_IdString == value)
                    return;
                _IdString = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region ProfileImageUri変更通知プロパティ
        private Uri _ProfileImageUri = null;

        public Uri ProfileImageUri
        {
            get
            { return _ProfileImageUri; }
            set
            {
                if (_ProfileImageUri == value)
                    return;
                _ProfileImageUri = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
