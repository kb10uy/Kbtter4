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

        public DirectMessageViewModel(DirectMessage dm, MainWindowViewModel main)
        {
            Source = dm;
            Text = Source.Text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
            CreatedAt = Source.CreatedAt.LocalDateTime.ToString();
            Sender = new UserViewModel(dm.Sender, main);
            IsSentByMe = dm.Sender.Id == Kbtter.Instance.AuthenticatedUser.Id;
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


    }
}
