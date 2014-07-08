using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace Kbtter4.Models
{
    public sealed class Kbtter4Setting : NotificationObject
    {
        public Kbtter4SettingConsumer Consumer { get; set; }

        public ObservableSynchronizedCollection<Kbtter4Account> Accounts { get; set; }

        public Kbtter4SettingTimelines Timelines { get; set; }

        public Kbtter4Setting()
        {
            Consumer = new Kbtter4SettingConsumer { Key = Kbtter4SettingConsumer.DefaultKey, Secret = Kbtter4SettingConsumer.DefaultSecret };
            Accounts = new ObservableSynchronizedCollection<Kbtter4Account>();
            Timelines = new Kbtter4SettingTimelines();
        }
    }

    public sealed class Kbtter4SettingConsumer : NotificationObject
    {

        public static readonly string DefaultKey = "5bI3XiTNEMHiamjMV5Acnqkex";
        public static readonly string DefaultSecret = "ni2jGjwKTLcdpp1x6nr3yFo9bRrSWRdZfYbzEAZLhKz4uDDErN";

        #region Key変更通知プロパティ
        private string _Key = "";

        public string Key
        {
            get
            { return _Key; }
            set
            {
                if (_Key == value)
                    return;
                _Key = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Secret変更通知プロパティ
        private string _Secret = "";

        public string Secret
        {
            get
            { return _Secret; }
            set
            {
                if (_Secret == value)
                    return;
                _Secret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }

    public sealed class Kbtter4SettingTimelines : NotificationObject
    {

        #region HomeStatusTimelineMax変更通知プロパティ
        private int _HomeStatusTimelineMax = 200;

        public int HomeStatusTimelineMax
        {
            get
            { return _HomeStatusTimelineMax; }
            set
            {
                if (_HomeStatusTimelineMax == value)
                    return;
                _HomeStatusTimelineMax = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeStatusTimelineInitialRead変更通知プロパティ
        private int _HomeStatusTimelineInitialRead = 100;

        public int HomeStatusTimelineInitialRead
        {
            get
            { return _HomeStatusTimelineInitialRead; }
            set
            {
                if (_HomeStatusTimelineInitialRead == value)
                    return;
                _HomeStatusTimelineInitialRead = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeNotificationTimelineMax変更通知プロパティ
        private int _HomeNotificationTimelineMax = 200;

        public int HomeNotificationTimelineMax
        {
            get
            { return _HomeNotificationTimelineMax; }
            set
            {
                if (_HomeNotificationTimelineMax == value)
                    return;
                _HomeNotificationTimelineMax = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeDirectMessageTimelineMax変更通知プロパティ
        private int _HomeDirectMessageTimelineMax = 30;

        public int HomeDirectMessageTimelineMax
        {
            get
            { return _HomeDirectMessageTimelineMax; }
            set
            {
                if (_HomeDirectMessageTimelineMax == value)
                    return;
                _HomeDirectMessageTimelineMax = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }
}
