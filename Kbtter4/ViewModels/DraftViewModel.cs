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
    public class DraftViewModel : ViewModel
    {
        public Kbtter4Draft Source { get; set; }

        public void Initialize()
        {
        }

        public DraftViewModel(Kbtter4Draft dr)
        {
            Source = dr;
            Text = dr.Text;
            CreatedDate = dr.CreatedDate;
            IsReply = dr.IsReply;
            InReplyToStatusId = dr.InReplyToStatusId;
            InReplyToStatusText = dr.InReplyToStatusOnelineText;
            InReplyToUserScreenName = dr.InReplyToUserScreenName;
            InReplyToUserProfileImageUri = dr.InReplyToUserProfileImageUri;
        }

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


        #region CreatedDate変更通知プロパティ
        private DateTime _CreatedDate;

        public DateTime CreatedDate
        {
            get
            { return _CreatedDate; }
            set
            {
                if (_CreatedDate == value)
                    return;
                _CreatedDate = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsReply変更通知プロパティ
        private bool _IsReply;

        public bool IsReply
        {
            get
            { return _IsReply; }
            set
            {
                if (_IsReply == value)
                    return;
                _IsReply = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region InReplyToStatusId変更通知プロパティ
        private long _InReplyToStatusId;

        public long InReplyToStatusId
        {
            get
            { return _InReplyToStatusId; }
            set
            {
                if (_InReplyToStatusId == value)
                    return;
                _InReplyToStatusId = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region InReplyToStatusText変更通知プロパティ
        private string _InReplyToStatusText;

        public string InReplyToStatusText
        {
            get
            { return _InReplyToStatusText; }
            set
            {
                if (_InReplyToStatusText == value)
                    return;
                _InReplyToStatusText = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region InReplyToUserScreenName変更通知プロパティ
        private string _InReplyToUserScreenName;

        public string InReplyToUserScreenName
        {
            get
            { return _InReplyToUserScreenName; }
            set
            {
                if (_InReplyToUserScreenName == value)
                    return;
                _InReplyToUserScreenName = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region InReplyToUserProfileImageUri変更通知プロパティ
        private Uri _InReplyToUserProfileImageUri;

        public Uri InReplyToUserProfileImageUri
        {
            get
            { return _InReplyToUserProfileImageUri; }
            set
            {
                if (_InReplyToUserProfileImageUri == value)
                    return;
                _InReplyToUserProfileImageUri = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public StatusViewModel CreateVirtualStatusViewModel(MainWindowViewModel mw)
        {
            return new StatusViewModel(mw, Source.CreateVirtualStatus(), true);
        }


        #region DeleteCommand
        private ViewModelCommand _DeleteCommand;

        public ViewModelCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new ViewModelCommand(Delete);
                }
                return _DeleteCommand;
            }
        }

        public void Delete()
        {
            Kbtter.Instance.AuthenticatedUserDrafts.Remove(Source);
        }
        #endregion

    }
}
