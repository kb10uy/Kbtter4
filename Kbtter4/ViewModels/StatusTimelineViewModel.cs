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
    public class StatusTimelineViewModel : ViewModel
    {
        public StatusTimelineViewModel()
        {

        }

        public StatusTimelineViewModel(MainWindowViewModel main, StatusTimeline tl)
        {
            Statuses = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                tl.Statuses,
                (p) =>
                {
                    if (!IsSelected) UnreadCount++;
                    return new StatusViewModel(main, p);
                },
                DispatcherHelper.UIDispatcher);
        }

        public void Initialize()
        {
        }


        #region Statuses変更通知プロパティ
        private ReadOnlyDispatcherCollection<StatusViewModel> _Statuses;

        public ReadOnlyDispatcherCollection<StatusViewModel> Statuses
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
                if (value) UnreadCount = 0;
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

    }
}
