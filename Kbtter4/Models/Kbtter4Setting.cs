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

        public Kbtter4Setting()
        {
            Consumer = new Kbtter4SettingConsumer { Key = Kbtter4SettingConsumer.DefaultKey, Secret = Kbtter4SettingConsumer.DefaultSecret };
            Accounts = new ObservableSynchronizedCollection<Kbtter4Account>();
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
}
