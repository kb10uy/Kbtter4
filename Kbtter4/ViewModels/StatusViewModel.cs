using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading.Tasks;

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

        public Status ReceivedStatus { get; private set; }
        private long rtid = 0;

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

            RegisterEventListeners();

            SourceStatus = st;
            ReceivedStatus = SourceStatus;
            if (SourceStatus.RetweetedStatus != null)
            {
                SourceStatus = SourceStatus.RetweetedStatus;
                IsRetweet = true;
                RetweetingUser = new UserViewModel(ReceivedStatus.User, main);
            }

            _IsFavorited = Kbtter.CheckFavorited(SourceStatus.Id);
            RaisePropertyChanged(() => IsFavorited);

            _IsRetweeted = Kbtter.CheckRetweeted(SourceStatus.Id);
            RaisePropertyChanged(() => IsRetweeted);

            _CreatedTimeText = SourceStatus.CreatedAt.LocalDateTime;
            RaisePropertyChanged(() => CreatedTimeText);

            User = new UserViewModel(SourceStatus.User, main);

            Text = SourceStatus.Text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
            OnelineText = Text
                .Replace("\r", " ")
                .Replace("\n", " ");

            FavoriteCount = SourceStatus.FavoriteCount ?? 0;
            RetweetCount = SourceStatus.RetweetCount ?? 0;

            IsMyStatus = SourceStatus.User.Id == Kbtter.AuthenticatedUser.Id;
            IsRetweetable = !IsMyStatus;

            ExtractVia();

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
                Task.Run(() =>
                {
                    try
                    {
                        if (value)
                        {
                            Kbtter.Token.Favorites.Create(id => SourceStatus.Id);
                        }
                        else
                        {
                            Kbtter.Token.Favorites.Destroy(id => SourceStatus.Id);
                        }
                    }
                    catch (TwitterException e)
                    {
                        main.View.Notify("お気に入り操作に失敗しました : " + e.Message);
                    }

                    _IsFavorited = value;
                    RaisePropertyChanged();
                });
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


        #region IsRetweeted変更通知プロパティ
        private bool _IsRetweeted;

        public bool IsRetweeted
        {
            get
            { return _IsRetweeted; }
            set
            {
                if (_IsRetweeted == value)
                    return;
                Task.Run(() =>
                {
                    try
                    {
                        if (value)
                        {
                            rtid = Kbtter.Token.Statuses.Retweet(id => SourceStatus.Id).Id;
                        }
                        else
                        {
                            if (rtid == 0)
                            {
                                rtid = Kbtter.Token.Statuses.Show(include_my_retweet => "true", id => SourceStatus.Id).Id;
                            }
                            Kbtter.Token.Favorites.Destroy(id => rtid);
                        }
                    }
                    catch (TwitterException e)
                    {
                        main.View.Notify("リツイート操作に失敗しました : " + e.Message);
                    }
                    _IsRetweeted = value;
                    RaisePropertyChanged();
                });
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


        #region CreatedTimeText変更通知プロパティ
        internal DateTime _CreatedTimeText;

        public string CreatedTimeText
        {
            get
            {
                var ts = (DateTime.Now - _CreatedTimeText);
                if (ts.Days >= 10)
                {
                    return _CreatedTimeText.ToString();
                }
                else if (ts.Days >= 1)
                {
                    return String.Format("{0}日前", ts.Days);
                }
                else if (ts.Hours >= 1)
                {
                    return String.Format("{0}時間前", ts.Hours);
                }
                else if (ts.Minutes >= 1)
                {
                    return String.Format("{0}分前", ts.Minutes);
                }
                else if (ts.Seconds >= 10)
                {
                    return String.Format("{0}秒前", ts.Seconds);
                }
                else
                {
                    return "今";
                }
            }
        }
        #endregion


        #region Via変更通知プロパティ
        private string _Via = "";

        public string Via
        {
            get
            { return _Via; }
            set
            {
                if (_Via == value)
                    return;
                _Via = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsMyStatus変更通知プロパティ
        private bool _IsMyStatus;

        public bool IsMyStatus
        {
            get
            { return _IsMyStatus; }
            set
            { 
                if (_IsMyStatus == value)
                    return;
                _IsMyStatus = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region DestroyStatusCommand
        private ViewModelCommand _DestroyStatusCommand;

        public ViewModelCommand DestroyStatusCommand
        {
            get
            {
                if (_DestroyStatusCommand == null)
                {
                    _DestroyStatusCommand = new ViewModelCommand(DestroyStatus, CanDestroyStatus);
                }
                return _DestroyStatusCommand;
            }
        }

        public bool CanDestroyStatus()
        {
            return IsMyStatus;
        }

        public async void DestroyStatus()
        {
            try
            {
                await Kbtter.Token.Statuses.DestroyAsync(id => SourceStatus.Id);
            }
            catch (TwitterException e)
            {
                main.View.Notify("ツイート削除に失敗しました : " + e.Message);
            }
        }
        #endregion


        #region ReplyCommand
        private ViewModelCommand _ReplyCommand;

        public ViewModelCommand ReplyCommand
        {
            get
            {
                if (_ReplyCommand == null)
                {
                    _ReplyCommand = new ViewModelCommand(Reply);
                }
                return _ReplyCommand;
            }
        }

        public void Reply()
        {
            main.SetReplyTo(this);
        }
        #endregion


        #region IsRetweetable変更通知プロパティ
        private bool _IsRetweetable;

        public bool IsRetweetable
        {
            get
            { return _IsRetweetable; }
            set
            { 
                if (_IsRetweetable == value)
                    return;
                _IsRetweetable = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region ユーティリティ

        static Regex reg = new Regex("<a href=\"(?<url>.+)\" rel=\"nofollow\">(?<client>.+)</a>");

        public void ExtractVia()
        {
            var m = reg.Match(SourceStatus.Source);
            if (!m.Success) return;

            Via = m.Groups["client"].Value;
        }

        public void RegisterEventListeners()
        {
            listener = new PropertyChangedEventListener(Kbtter);
            CompositeDisposable.Add(listener);
            listener.Add("Favorites", (s, e) =>
            {
                Task.Run(() =>
                {
                    _IsFavorited = Kbtter.CheckFavorited(SourceStatus.Id);
                    RaisePropertyChanged(() => IsFavorited);
                    RaisePropertyChanged(() => CreatedTimeText);
                });
            });
            listener.Add("Retweets", (s, e) =>
            {
                Task.Run(() =>
                {
                    _IsRetweeted = Kbtter.CheckRetweeted(SourceStatus.Id);
                    RaisePropertyChanged(() => IsRetweeted);
                    RaisePropertyChanged(() => CreatedTimeText);
                });
            });

            listener.Add("Statuses", (s, e) =>
            {
                RaisePropertyChanged(() => CreatedTimeText);
            });
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
