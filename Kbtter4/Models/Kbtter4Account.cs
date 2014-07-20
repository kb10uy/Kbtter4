using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;


namespace Kbtter4.Models
{
    public sealed class Kbtter4Account : NotificationObject
    {

        public Kbtter4Account()
        {
            AccessToken = "";
            AccessTokenSecret = "";
            ScreenName = "";
            Timelines = new ObservableSynchronizedCollection<Kbtter4SettingStatusTimelineData>();
            Drafts = new ObservableSynchronizedCollection<Kbtter4Draft>();
            UserId = 0;
        }

        #region AccessToken変更通知プロパティ
        private string _AccessToken;

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
        private string _AccessTokenSecret;

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

        #region UserId変更通知プロパティ
        private long _UserId;

        public long UserId
        {
            get
            { return _UserId; }
            set
            { 
                if (_UserId == value)
                    return;
                _UserId = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Timelines変更通知プロパティ
        private ObservableSynchronizedCollection<Kbtter4SettingStatusTimelineData> _Timelines;

        public ObservableSynchronizedCollection<Kbtter4SettingStatusTimelineData> Timelines
        {
            get
            { return _Timelines; }
            set
            { 
                if (_Timelines == value)
                    return;
                _Timelines = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Drafts変更通知プロパティ
        private ObservableSynchronizedCollection<Kbtter4Draft> _Drafts;

        public ObservableSynchronizedCollection<Kbtter4Draft> Drafts
        {
            get
            { return _Drafts; }
            set
            { 
                if (_Drafts == value)
                    return;
                _Drafts = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TegakiPenthickness変更通知プロパティ
        private double _TegakiPenthickness;

        public double TegakiPenthickness
        {
            get
            { return _TegakiPenthickness; }
            set
            { 
                if (_TegakiPenthickness == value)
                    return;
                _TegakiPenthickness = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }
}
