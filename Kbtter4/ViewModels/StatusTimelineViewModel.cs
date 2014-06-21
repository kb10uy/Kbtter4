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

        public StatusTimelineViewModel(StatusTimeline tl)
        {
            ViewModelHelper.CreateReadOnlyDispatcherCollection(tl.Statuses, (p) => new StatusViewModel(p), DispatcherHelper.UIDispatcher);
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

    }
}
