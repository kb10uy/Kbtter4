using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using CoreTweet;

namespace Kbtter4.Models
{
    public sealed class Kbtter4User : NotificationObject
    {
        private User SourceUser { get; set; }

        public Kbtter4User()
        {

        }

        public Kbtter4User(User user)
        {
            SourceUser = user;
            Name = SourceUser.Name;
            ScreenName = SourceUser.ScreenName;
            Id = SourceUser.Id ?? 0;
            ProfileImageUri = SourceUser.ProfileImageUrlHttps;
            RaisePropertyChanged();
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


        #region ScreenName変更通知プロパティ
        private string _ScreenName;

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
        private long _Id;

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


        #region ProfileImageUri変更通知プロパティ
        private Uri _ProfileImageUri;

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
