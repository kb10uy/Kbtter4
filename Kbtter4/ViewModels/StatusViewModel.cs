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
            User = new UserViewModel(st.User);
            Text = SourceStatus.Text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
            OnelineText = Text
                .Replace("\r", " ")
                .Replace("\n", " ");

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
                FavoriteIcon = value ? Kbtter4NotificationIconKind.Favorited : Kbtter4NotificationIconKind.Unfavorited;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region FavoriteIcon変更通知プロパティ
        private Kbtter4NotificationIconKind _FavoriteIcon = Kbtter4NotificationIconKind.Unfavorited;

        public Kbtter4NotificationIconKind FavoriteIcon
        {
            get
            { return _FavoriteIcon; }
            set
            {
                if (_FavoriteIcon == value)
                    return;
                _FavoriteIcon = value;
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
