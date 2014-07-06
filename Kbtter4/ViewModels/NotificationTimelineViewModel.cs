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
    public class NotificationTimelineViewModel : ViewModel
    {

        public NotificationTimelineViewModel(MainWindowViewModel main, NotificationTimeline tl)
        {
            Notificaitons = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                tl.Notifications,
                (p) =>
                {
                    if (!IsSelected) UnreadCount++;
                    return new NotificationViewModel(p, main);
                },
                DispatcherHelper.UIDispatcher);
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
                if (value)
                {
                    UnreadCount = 0;
                }
                RaisePropertyChanged();
            }
        }
        #endregion


        #region UnreadCount変更通知プロパティ
        private int _UnreadCount = 0;

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


        #region Notificaitons変更通知プロパティ
        private ReadOnlyDispatcherCollection<NotificationViewModel> _Notificaitons;

        public ReadOnlyDispatcherCollection<NotificationViewModel> Notificaitons
        {
            get
            { return _Notificaitons; }
            set
            {
                if (_Notificaitons == value)
                    return;
                _Notificaitons = value;
                RaisePropertyChanged();
            }
        }
        #endregion


    }
}
