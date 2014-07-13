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
    public class DirectMessageViewModel : ViewModel
    {
        public DirectMessage Source { get; private set; }

        MainWindowViewModel main;
        Kbtter Kbtter;

        public DirectMessageViewModel(DirectMessage dm, MainWindowViewModel main)
        {
            Kbtter = Kbtter.Instance;
            this.main = main;
            Source = dm;
            Text = Source.Text;
            CreatedAt = Source.CreatedAt.LocalDateTime.ToString();
            Sender = new UserViewModel(dm.Sender, main);
            IsSentByMe = dm.Sender.Id == Kbtter.Instance.AuthenticatedUser.Id;
            AnalyzeTextElements();
        }

        public void Initialize()
        {
        }


        #region IsSentByMe変更通知プロパティ
        private bool _IsSentByMe;

        public bool IsSentByMe
        {
            get
            { return _IsSentByMe; }
            set
            { 
                if (_IsSentByMe == value)
                    return;
                _IsSentByMe = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Sender変更通知プロパティ
        private UserViewModel _Sender;

        public UserViewModel Sender
        {
            get
            { return _Sender; }
            set
            {
                if (_Sender == value)
                    return;
                _Sender = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Text変更通知プロパティ
        private string _Text;

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


        #region CreatedAt変更通知プロパティ
        private string _CreatedAt;

        public string CreatedAt
        {
            get
            { return _CreatedAt; }
            set
            {
                if (_CreatedAt == value)
                    return;
                _CreatedAt = value;
                RaisePropertyChanged();
            }
        }
        #endregion


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


        #region DeleteCommand
        private ViewModelCommand _DeleteCommand;

        public ViewModelCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new ViewModelCommand(Delete, CanDelete);
                }
                return _DeleteCommand;
            }
        }

        public bool CanDelete()
        {
            return IsSentByMe;
        }

        public async void Delete()
        {
            await Kbtter.Token.DirectMessages.DestroyAsync(id => Source.Id);
        }
        #endregion


        private void AnalyzeTextElements()
        {
            TextElements = new ObservableSynchronizedCollection<StatusTextElement>();
            var l = new List<Tuple<int[], StatusTextElement>>();

            if (Source.Entities != null)
            {
                if (Source.Entities.Urls != null)
                    foreach (var i in Source.Entities.Urls)
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

                if (Source.Entities.Media != null)
                    foreach (var i in Source.Entities.Media)
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

                if (Source.Entities.UserMentions != null)
                    foreach (var i in Source.Entities.UserMentions)
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

                if (Source.Entities.HashTags != null)
                    foreach (var i in Source.Entities.HashTags)
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
    }
}
