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
    public class DirectMessageTimelineViewModel : ViewModel
    {

        public DirectMessageTimelineViewModel(MainWindowViewModel main, DirectMessageTimeline dtl)
        {
            DirectMessages = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                dtl.DirectMessages,
                (p) =>
                {
                    if (!IsSelected) UnreadCount++;
                    return new DirectMessageViewModel(p, main);
                },
                DispatcherHelper.UIDispatcher);
            Party = new UserViewModel(dtl.Party, main);
        }

        public void Initialize()
        {
        }


        #region IsSelected変更通知プロパティ
        private bool _IsSelected;

        public bool IsSelected
        {
            get
            { return _IsSelected; }
            set
            { 
                if (_IsSelected == value)
                    return;
                _IsSelected = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region UnreadCount変更通知プロパティ
        private int _UnreadCount;

        public int UnreadCount
        {
            get
            { return _UnreadCount; }
            set
            { 
                if (_UnreadCount == value)
                    return;
                _UnreadCount = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region DirectMessages変更通知プロパティ
        private ReadOnlyDispatcherCollection<DirectMessageViewModel> _DirectMessages;

        public ReadOnlyDispatcherCollection<DirectMessageViewModel> DirectMessages
        {
            get
            { return _DirectMessages; }
            set
            { 
                if (_DirectMessages == value)
                    return;
                _DirectMessages = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Party変更通知プロパティ
        private UserViewModel _Party;

        public UserViewModel Party
        {
            get
            { return _Party; }
            set
            { 
                if (_Party == value)
                    return;
                _Party = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }
}
