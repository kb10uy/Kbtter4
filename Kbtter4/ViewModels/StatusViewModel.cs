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
using Kbtter4.Ayaya;
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

        public StatusViewModel(MainWindowViewModel mw, Status st, bool isinc)
        {
            Kbtter = Kbtter.Instance;
            main = mw;
            SourceStatus = st;
            OnelineText = st.Text;
            User = new UserViewModel(st.User, mw);
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

            Text = SourceStatus.Text;

            OnelineText = Text
                .Replace("\r", " ")
                .Replace("\n", " ");

            FavoriteCount = SourceStatus.FavoriteCount ?? 0;
            RetweetCount = SourceStatus.RetweetCount ?? 0;

            IsMyStatus = SourceStatus.User.Id == Kbtter.AuthenticatedUser.Id;
            IsRetweetable = !IsMyStatus && !SourceStatus.User.IsProtected;

            if (SourceStatus.Entities.UserMentions != null)
            {
                IsReplyToMe = SourceStatus.Entities.UserMentions.Any(p => p.Id == Kbtter.AuthenticatedUser.Id);
            }
            ExtractVia();
            AnalyzeTextElements();

            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                if (SourceStatus.Entities != null)
                {
                    Medias = new ObservableSynchronizedCollection<StatusMediaViewModel>();
                    if (SourceStatus.Entities.Urls != null)
                    {
                        var r = Kbtter4ExtraMediaUriConverter.TryGetDirectUri(SourceStatus.Entities.Urls.Select(p => p.ExpandedUrl));
                        foreach (var i in r) Medias.Add(new StatusMediaViewModel { Uri = i });
                    }
                    if (SourceStatus.Entities.Media != null)
                    {
                        foreach (var i in SourceStatus.Entities.Media)
                        {
                            Medias.Add(new StatusMediaViewModel { Uri = i.MediaUrlHttps });
                        }
                    }

                    HasMedia = Medias.Count != 0;

                }
            }));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CompositeDisposable.Dispose();
        }

        #region TextElements変更通知プロパティ
        private ObservableSynchronizedCollection<StatusTextElement> _TextElements;

        public ObservableSynchronizedCollection<StatusTextElement> TextElements
        {
            get
            { return _TextElements; }
            set
            {
                if (_TextElements == value)
                    return;
                _TextElements = value;
                RaisePropertyChanged();
            }
        }
        #endregion


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
                        if (!Enum.IsDefined(typeof(ErrorCode), e.Errors[0].Code)) return;
                        switch ((ErrorCode)e.Errors[0].Code)
                        {
                            case ErrorCode.AlreadyFavorited:
                                Kbtter.AddFavorite(SourceStatus);
                                main.View.Notify("すでにお気に入り登録済みです");
                                break;
                            case ErrorCode.PageDoesNotExist:
                                Kbtter.RemoveFavorite(SourceStatus.Id);
                                main.View.Notify("お気に入りに登録されていません");
                                break;
                            default:
                                break;
                        }
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


        #region IsReplyToMe変更通知プロパティ
        private bool _IsReplyToMe;

        public bool IsReplyToMe
        {
            get
            { return _IsReplyToMe; }
            set
            {
                if (_IsReplyToMe == value)
                    return;
                _IsReplyToMe = value;
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


        #region HasMedia変更通知プロパティ
        private bool _HasMedia;

        public bool HasMedia
        {
            get
            { return _HasMedia; }
            set
            {
                if (_HasMedia == value)
                    return;
                _HasMedia = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Medias変更通知プロパティ
        private ObservableSynchronizedCollection<StatusMediaViewModel> _Medias;

        public ObservableSynchronizedCollection<StatusMediaViewModel> Medias
        {
            get
            { return _Medias; }
            set
            {
                if (_Medias == value)
                    return;
                _Medias = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        bool gotexm = false;

        #region GetExtendedMediaCommand
        private ViewModelCommand _GetExtendedMediaCommand;

        public ViewModelCommand GetExtendedMediaCommand
        {
            get
            {
                if (_GetExtendedMediaCommand == null)
                {
                    _GetExtendedMediaCommand = new ViewModelCommand(GetExtendedMedia, CanGetExtendedMedia);
                }
                return _GetExtendedMediaCommand;
            }
        }

        public bool CanGetExtendedMedia()
        {
            return HasMedia && !gotexm;
        }

        public async void GetExtendedMedia()
        {
            try
            {
                var nst = await Kbtter.Token.Statuses.ShowAsync(id => SourceStatus.Id);
                if (nst.ExtendedEntities == null)
                {
                    main.View.Notify("extended_entitiesが取得できませんでした。");
                }
                if (nst.ExtendedEntities.Media == null)
                {
                    main.View.Notify("mediaが取得できませんでした。");
                }
                Medias.Clear();
                foreach (var i in nst.ExtendedEntities.Media)
                {
                    Medias.Add(new StatusMediaViewModel { Uri = i.MediaUrlHttps });
                }
                var r = Kbtter4ExtraMediaUriConverter.TryGetDirectUri(SourceStatus.Entities.Urls.Select(p => p.ExpandedUrl));
                foreach (var i in r) Medias.Add(new StatusMediaViewModel { Uri = i });
                gotexm = true;
                GetExtendedMediaCommand.RaiseCanExecuteChanged();
            }
            catch (TwitterException e)
            {
                main.View.Notify("ツイートの取得に失敗しました : " + e.Message);
            }
            catch { }
        }
        #endregion


        #region OpenViaCommand
        private ViewModelCommand _OpenViaCommand;

        public ViewModelCommand OpenViaCommand
        {
            get
            {
                if (_OpenViaCommand == null)
                {
                    _OpenViaCommand = new ViewModelCommand(OpenVia);
                }
                return _OpenViaCommand;
            }
        }

        public void OpenVia()
        {
            if (viauri != null) main.View.OpenInDefault(viauri);
        }
        #endregion


        #region OpenStatusLinkCommand
        private ViewModelCommand _OpenStatusLinkCommand;

        public ViewModelCommand OpenStatusLinkCommand
        {
            get
            {
                if (_OpenStatusLinkCommand == null)
                {
                    _OpenStatusLinkCommand = new ViewModelCommand(OpenStatusLink);
                }
                return _OpenStatusLinkCommand;
            }
        }

        public void OpenStatusLink()
        {
            main.View.OpenInDefault(string.Format("https://twitter.com/{0}/status/{1}", User.ScreenName, SourceStatus.Id));
        }
        #endregion


        #region ユーティリティ

        static Regex reg = new Regex("<a href=\"(?<url>.+)\" rel=\"nofollow\">(?<client>.+)</a>");
        Uri viauri;

        public void ExtractVia()
        {
            var m = reg.Match(SourceStatus.Source);
            if (!m.Success) return;

            Via = m.Groups["client"].Value;
            try
            {
                viauri = new Uri(m.Groups["url"].Value);
            }
            catch
            {
                viauri = null;
            }
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

        private void AnalyzeTextElements()
        {
            TextElements = new ObservableSynchronizedCollection<StatusTextElement>();
            var l = new List<Tuple<int[], StatusTextElement>>();

            if (SourceStatus.Entities != null)
            {
                if (SourceStatus.Entities.Urls != null)
                    foreach (var i in SourceStatus.Entities.Urls)
                    {
                        //Text = Text.Replace(i.Url.ToString(), i.DisplayUrl.ToString());
                        var e = new StatusTextElement();
                        e.Original = i.Url.ToString();
                        e.Action = main.View.OpenInDefault;
                        e.Type = StatusTextElementType.Uri;
                        e.Link = i.ExpandedUrl;
                        e.Surface = i.DisplayUrl;
                        l.Add(new Tuple<int[], StatusTextElement>(i.Indices, e));
                    }

                if (SourceStatus.Entities.Media != null)
                    foreach (var i in SourceStatus.Entities.Media)
                    {
                        //Text = Text.Replace(i.Url.ToString(), i.DisplayUrl.ToString());
                        var e = new StatusTextElement();
                        e.Original = i.Url.ToString();
                        e.Action = main.View.OpenInDefault;
                        e.Type = StatusTextElementType.Media;
                        e.Link = i.ExpandedUrl;
                        e.Surface = i.DisplayUrl;
                        l.Add(new Tuple<int[], StatusTextElement>(i.Indices, e));
                    }

                if (SourceStatus.Entities.UserMentions != null)
                    foreach (var i in SourceStatus.Entities.UserMentions)
                    {
                        var e = new StatusTextElement();
                        e.Action = async (p) =>
                        {
                            var user = await Kbtter.Token.Users.ShowAsync(id => i.Id);
                            Kbtter.AddUserToUsersList(user);
                            main.View.Notify(user.Name + "さんの情報");
                            main.View.ChangeToUser();
                        };
                        e.Type = StatusTextElementType.User;
                        e.Link = new Uri("https://twitter.com/" + i.ScreenName);
                        e.Surface = "@" + i.ScreenName;
                        e.Original = e.Surface;
                        l.Add(new Tuple<int[], StatusTextElement>(i.Indices, e));
                    }

                if (SourceStatus.Entities.HashTags != null)
                    foreach (var i in SourceStatus.Entities.HashTags)
                    {
                        var e = new StatusTextElement();
                        e.Action = (p) =>
                        {
                            main.View.ChangeToSearch();
                            main.View.SearchText = "#" + i.Text;
                            Kbtter.Search("#" + i.Text);
                        };
                        e.Type = StatusTextElementType.Hashtag;
                        e.Link = new Uri("https://twitter.com/search?q=%23" + i.Text);
                        e.Surface = "#" + i.Text;
                        e.Original = e.Surface;
                        l.Add(new Tuple<int[], StatusTextElement>(i.Indices, e));
                    }

                l.Sort((x, y) => x.Item1[0].CompareTo(y.Item1[0]));
            }

            int le = 0;
            foreach (var i in l)
            {
                var el = i.Item1[1] - i.Item1[0];
                var ntl = i.Item1[0] - le;
                if (ntl != 0)
                {
                    var nt = Text.Substring(le, ntl);
                    nt = nt
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&");
                    TextElements.Add(new StatusTextElement { Surface = nt, Type = StatusTextElementType.None });
                }
                TextElements.Add(i.Item2);
                le = i.Item1[1];
            }
            //foreach (var i in l) Text = Text.Replace(i.Item2.Original, i.Item2.Surface);
            if (Text.Length > le - 1)
            {
                var ls = Text.Substring(le);
                ls = ls
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&");
                TextElements.Add(new StatusTextElement { Surface = ls, Type = StatusTextElementType.None });
            }
        }

        #endregion

    }

    public sealed class StatusTextElement
    {
        public string Surface { get; set; }
        public string Internal { get; set; }
        public Uri Link { get; set; }
        public StatusTextElementType Type { get; set; }
        public Action<string> Action { get; set; }
        public string Original { get; set; }
    }

    public enum StatusTextElementType
    {
        None,
        Uri,
        Media,
        User,
        Hashtag,
    }

}
