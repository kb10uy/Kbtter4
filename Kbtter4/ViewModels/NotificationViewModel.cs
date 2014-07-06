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

namespace Kbtter4.ViewModels
{
    public class NotificationViewModel : ViewModel
    {
        public NotificationViewModel(Kbtter4Notification nt, MainWindowViewModel mw)
        {
            var ins = Kbtter.Instance;
            switch (nt.Kind)
            {
                case Kbtter4NotificationKind.Favorited:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("ツイートが{0}さんのお気に入りに登録されました", SourceUser.Name);
                    IconKind = Kbtter4NotificationIconKind.Favorited;
                    Description = nt.SourceEvent.TargetStatus.Text;
                    break;
                case Kbtter4NotificationKind.Unfavorited:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("ツイートが{0}さんのお気に入りから削除されました", SourceUser.Name);
                    IconKind = Kbtter4NotificationIconKind.Unfavorited;
                    Description = nt.SourceEvent.TargetStatus.Text;
                    break;
                case Kbtter4NotificationKind.Followed:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("{0}さんにフォローされました", SourceUser.Name);
                    IconKind = Kbtter4NotificationIconKind.Followed;
                    Description = nt.SourceEvent.Source.Description;
                    break;
                case Kbtter4NotificationKind.Unfollowed:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("{0}さんにリムーブされました", SourceUser.Name);
                    IconKind = Kbtter4NotificationIconKind.Unfollowed;
                    Description = nt.SourceEvent.Source.Description;
                    break;
                case Kbtter4NotificationKind.Retweeted:
                    SourceUser = new UserViewModel(nt.SourceStatus.Status.User, mw);
                    if (nt.SourceStatus.Status.Text.Contains(ins.AuthenticatedUser.ScreenName) &&
                        nt.SourceStatus.Status.RetweetedStatus.User.Id != ins.AuthenticatedUser.Id)
                    {
                        Message = string.Format("リツイートが{0}さんにリツイートされました", SourceUser.Name);
                    }
                    else
                    {
                        Message = string.Format("ツイートが{0}さんにリツイートされました", SourceUser.Name);
                    }
                    IconKind = Kbtter4NotificationIconKind.Retweeted;
                    Description = nt.SourceStatus.Status.RetweetedStatus.Text;
                    break;
                case Kbtter4NotificationKind.ListAdded:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("{0}さんのリスト {1} に登録されました", SourceUser.Name, nt.SourceEvent.TargetList.Name);
                    IconKind = Kbtter4NotificationIconKind.ListAdded;
                    Description = nt.SourceEvent.TargetList.Description;
                    break;
                case Kbtter4NotificationKind.ListRemoved:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("{0}さんのリスト {1} から外されました", SourceUser.Name, nt.SourceEvent.TargetList.Name);
                    IconKind = Kbtter4NotificationIconKind.ListRemoved;
                    Description = nt.SourceEvent.TargetList.Description;
                    break;
                case Kbtter4NotificationKind.Blocked:
                    SourceUser = new UserViewModel(nt.SourceEvent.Source, mw);
                    Message = string.Format("{0}さんにブロックされました", SourceUser.Name);
                    IconKind = Kbtter4NotificationIconKind.Blocked;
                    Description = nt.SourceEvent.Source.Description;
                    break;
            }
        }

        public void Initialize()
        {
        }


        #region SourceUser変更通知プロパティ
        private UserViewModel _SourceUser = new UserViewModel();

        public UserViewModel SourceUser
        {
            get
            { return _SourceUser; }
            set
            {
                if (_SourceUser == value)
                    return;
                _SourceUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Message変更通知プロパティ
        private string _Message = "";

        public string Message
        {
            get
            { return _Message; }
            set
            {
                if (_Message == value)
                    return;
                _Message = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Description変更通知プロパティ
        private string _Description = "";

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


        #region IconKind変更通知プロパティ
        private Kbtter4NotificationIconKind _IconKind;

        public Kbtter4NotificationIconKind IconKind
        {
            get
            { return _IconKind; }
            set
            {
                if (_IconKind == value)
                    return;
                _IconKind = value;
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
