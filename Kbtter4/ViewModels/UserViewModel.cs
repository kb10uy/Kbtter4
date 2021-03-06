﻿using System;
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
        Kbtter Kbtter;
        User src;
        MainWindowViewModel main;
        //PropertyChangedEventListener listener;

        public UserViewModel(User user, MainWindowViewModel mw)
        {
            Kbtter = Kbtter.Instance;
            main = mw;
            src = user;
            Name = src.Name;
            ScreenName = src.ScreenName;
            IdString = src.Id.ToString();
            ProfileImageUri = src.ProfileImageUrlHttps;
            IsProtected = src.IsProtected;
            Location = src.Location;
            UriString = src.Url != null ? src.Url.ToString() : "";
            Description = src.Description;
            Statuses = src.StatusesCount;
            Favorites = src.FavouritesCount;
            Friends = src.FriendsCount;
            Followers = src.FollowersCount;
            //if (Description != null) AnalyzeDescription();
        }
        /*
        //UserStreamのStatus.UserにEntityがろくに含まれてないので凍結
        public void AnalyzeDescription()
        {
            var l = new List<Tuple<int[], StatusTextElement>>();

            if (src.Entities != null && src.Entities.Url != null)
            {
                if (src.Entities.Url.Urls != null)
                    foreach (var i in src.Entities.Url.Urls)
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

                if (src.Entities.Url.Media != null)
                    foreach (var i in src.Entities.Url.Media)
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

                if (src.Entities.Url.UserMentions != null)
                    foreach (var i in src.Entities.Url.UserMentions)
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

                if (src.Entities.Url.HashTags != null)
                    foreach (var i in src.Entities.Url.HashTags)
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
                    var nt = Description.Substring(le, ntl);
                    nt = nt
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&");
                    DescriptionElements.Add(new StatusTextElement { Surface = nt, Type = StatusTextElementType.None });
                }
                DescriptionElements.Add(i.Item2);
                le = i.Item1[1];
            }
            //foreach (var i in l) Text = Text.Replace(i.Item2.Original, i.Item2.Surface);
            if (Description.Length > le - 1)
            {
                var ls = Description.Substring(le);
                ls = ls
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&");
                DescriptionElements.Add(new StatusTextElement { Surface = ls, Type = StatusTextElementType.None });
            }
        }
        */
        public UserViewModel(User user, MainWindowViewModel mw, bool gfs)
            : this(user, mw)
        {

        }

        public UserViewModel()
        {
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


        #region IsProtected変更通知プロパティ
        private bool _IsProtected;

        public bool IsProtected
        {
            get
            { return _IsProtected; }
            set
            {
                if (_IsProtected == value)
                    return;
                _IsProtected = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Location変更通知プロパティ
        private string _Location;

        public string Location
        {
            get
            { return _Location; }
            set
            {
                if (_Location == value)
                    return;
                _Location = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region UriString変更通知プロパティ
        private string _UriString;

        public string UriString
        {
            get
            { return _UriString; }
            set
            {
                if (_UriString == value)
                    return;
                _UriString = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Description変更通知プロパティ
        private string _Description;

        public string Description
        {
            get
            { return _Description; }
            set
            {
                if (_Description == value)
                    return;
                _Description = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region DescriptionElements変更通知プロパティ
        private ObservableSynchronizedCollection<StatusTextElement> _DescriptionElements = new ObservableSynchronizedCollection<StatusTextElement>();

        public ObservableSynchronizedCollection<StatusTextElement> DescriptionElements
        {
            get
            { return _DescriptionElements; }
            set
            {
                if (_DescriptionElements == value)
                    return;
                _DescriptionElements = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsFollowingMe変更通知プロパティ
        private bool _IsFollowingMe;

        public bool IsFollowingMe
        {
            get
            { return _IsFollowingMe; }
            set
            {
                if (_IsFollowingMe == value)
                    return;
                _IsFollowingMe = value;
                RaisePropertyChanged("IsFollowingMe");
            }
        }
        #endregion


        #region IsFollowedbyMe変更通知プロパティ
        private bool _IsFollowedbyMe;

        public bool IsFollowedbyMe
        {
            get
            { return _IsFollowedbyMe; }
            set
            {
                if (_IsFollowedbyMe == value)
                    return;
                _IsFollowedbyMe = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsBlockingMe変更通知プロパティ
        private bool _IsBlockingMe;

        public bool IsBlockingMe
        {
            get
            { return _IsBlockingMe; }
            set
            {
                if (_IsBlockingMe == value)
                    return;
                _IsBlockingMe = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsBlockedByMe変更通知プロパティ
        private bool _IsBlockedByMe;

        public bool IsBlockedByMe
        {
            get
            { return _IsBlockedByMe; }
            set
            {
                if (_IsBlockedByMe == value)
                    return;
                _IsBlockedByMe = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Statuses変更通知プロパティ
        private int _Statuses;

        public int Statuses
        {
            get
            { return _Statuses; }
            set
            {
                if (_Statuses == value)
                    return;
                _Statuses = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Favorites変更通知プロパティ
        private int _Favorites;

        public int Favorites
        {
            get
            { return _Favorites; }
            set
            {
                if (_Favorites == value)
                    return;
                _Favorites = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Friends変更通知プロパティ
        private int _Friends;

        public int Friends
        {
            get
            { return _Friends; }
            set
            {
                if (_Friends == value)
                    return;
                _Friends = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Followers変更通知プロパティ
        private int _Followers;

        public int Followers
        {
            get
            { return _Followers; }
            set
            {
                if (_Followers == value)
                    return;
                _Followers = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region OpenUriCommand
        private ViewModelCommand _OpenUriCommand;

        public ViewModelCommand OpenUriCommand
        {
            get
            {
                if (_OpenUriCommand == null)
                {
                    _OpenUriCommand = new ViewModelCommand(OpenUri);
                }
                return _OpenUriCommand;
            }
        }

        public void OpenUri()
        {
            if (src.Url != null) main.View.OpenInDefault(src.Url);
        }
        #endregion


        #region ShowUserInformationCommand
        private ViewModelCommand _ShowUserInformationCommand;

        public ViewModelCommand ShowUserInformationCommand
        {
            get
            {
                if (_ShowUserInformationCommand == null)
                {
                    _ShowUserInformationCommand = new ViewModelCommand(ShowUserInformation);
                }
                return _ShowUserInformationCommand;
            }
        }

        public void ShowUserInformation()
        {
            Kbtter.AddUserToUsersList(src);
            main.View.Notify(Name + "さんの情報");
            main.View.ChangeToUser();
        }
        #endregion

        bool fsgot;
        public async void GetFriendship()
        {
            if (fsgot) return;
            try
            {
                var fs = await Kbtter.Token.Friendships.ShowAsync(source_id => Kbtter.AuthenticatedUser.Id, target_id => src.Id);
                IsFollowedbyMe = fs.Target.IsFollowedBy ?? false;
                IsFollowingMe = fs.Target.IsFollowing ?? false;
                IsBlockedByMe = fs.Source.IsBlocking ?? false;
                IsBlockingMe = fs.Target.IsBlocking ?? false;
                fsgot = true;
                return;
            }
            catch (TwitterException e)
            {
                main.View.Notify("フォロー関係を取得できませんでした : " + e.Message);
            }
        }
    }
}
