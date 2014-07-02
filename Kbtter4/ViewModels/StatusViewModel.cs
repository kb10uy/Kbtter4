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
    public class StatusViewModel : ViewModel
    {

        public Status SourceStatus { get; private set; }

        private Status rted;

        public Kbtter Kbtter;
        public MainWindowViewModel main;
        public PropertyChangedEventListener listener;

        public void Initialize()
        {
        }

        public StatusViewModel(MainWindowViewModel mw, Status st)
        {
            Kbtter = Kbtter.Instance;
            main = mw;
            listener = new PropertyChangedEventListener(Kbtter);
            CompositeDisposable.Add(listener);
            listener.Add("Favorites", (s, e) =>
            {
                _IsFavorited = Kbtter.CheckFavorited(SourceStatus.Id);
                RaisePropertyChanged(() => IsFavorited);
            });

            SourceStatus = st;
            if (SourceStatus.RetweetedStatus != null)
            {
                rted = SourceStatus;
                SourceStatus = SourceStatus.RetweetedStatus;
                IsRetweet = true;
                RetweetingUser = new UserViewModel(rted.User);
            }

            _IsFavorited = Kbtter.CheckFavorited(SourceStatus.Id);
            RaisePropertyChanged(() => IsFavorited);
            
            User = new UserViewModel(SourceStatus.User);
            Text = SourceStatus.Text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
            OnelineText = Text
                .Replace("\r", " ")
                .Replace("\n", " ");
            FavoriteCount = SourceStatus.FavoriteCount ?? 0;
            RetweetCount = SourceStatus.RetweetCount ?? 0;

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CompositeDisposable.Dispose();
        }


        #region User変更通知プロパティ
        private UserViewModel _User;

        public UserViewModel User
        {
            get
            { return _User; }
            set
            {
                if (_User == value)
                    return;
                _User = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Text変更通知プロパティ
        private string _Text = "";

        public string Text
        {
            get
            { return _Text; }
            set
            {
                if (_Text == value)
                    return;
                _Text = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region OnelineText変更通知プロパティ
        private string _OnelineText = "";

        public string OnelineText
        {
            get
            { return _OnelineText; }
            set
            {
                if (_OnelineText == value)
                    return;
                _OnelineText = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsFavorited変更通知プロパティ
        private bool _IsFavorited;

        public bool IsFavorited
        {
            get
            { return _IsFavorited; }
            set
            {
                if (_IsFavorited == value)
                    return;
                _IsFavorited = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsRetweet変更通知プロパティ
        private bool _IsRetweet;

        public bool IsRetweet
        {
            get
            { return _IsRetweet; }
            set
            {
                if (_IsRetweet == value)
                    return;
                _IsRetweet = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region RetweetingUser変更通知プロパティ
        private UserViewModel _RetweetingUser = new UserViewModel();

        public UserViewModel RetweetingUser
        {
            get
            { return _RetweetingUser; }
            set
            {
                if (_RetweetingUser == value)
                    return;
                _RetweetingUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region FavoriteCount変更通知プロパティ
        private long _FavoriteCount;

        public long FavoriteCount
        {
            get
            { return _FavoriteCount; }
            set
            {
                if (_FavoriteCount == value)
                    return;
                _FavoriteCount = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region RetweetCount変更通知プロパティ
        private long _RetweetCount;

        public long RetweetCount
        {
            get
            { return _RetweetCount; }
            set
            {
                if (_RetweetCount == value)
                    return;
                _RetweetCount = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }

    public enum Kbtter4NotificationIconKind
    {
        Undefined,
        None,
        Favorited,
        Unfavorited,
        Followed,
        Unfollowed,
        Retweeted,
        Blocked,
        Unblocked,
        ListAdded,
        ListRemoved,
    }
}
