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
        public StatusTimeline Source { get; private set; }

        public StatusTimelineViewModel()
        {

        }

        public StatusTimelineViewModel(MainWindowViewModel main, StatusTimeline tl)
        {
            Source = tl;
            Statuses = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                tl.Statuses,
                (p) =>
                {
                    if (!IsSelected) UnreadCount++;
                    return new StatusViewModel(main, p);
                },
                DispatcherHelper.UIDispatcher);
            Name = Source.Name;
            QueryText = Source.Query.QueryText;
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


        #region DeleteStatusesCommand
        private ViewModelCommand _DeleteStatusesCommand;

        public ViewModelCommand DeleteStatusesCommand
        {
            get
            {
                if (_DeleteStatusesCommand == null)
                {
                    _DeleteStatusesCommand = new ViewModelCommand(DeleteStatuses);
                }
                return _DeleteStatusesCommand;
            }
        }

        public void DeleteStatuses()
        {
            Source.Statuses.Clear();
            UnreadCount = 0;
        }
        #endregion


        #region Name変更通知プロパティ
        private string _Name;

        public string Name
        {
            get
            { return _Name; }
            set
            {
                if (_Name == value)
                    return;
                _Name = value;
                Source.Name = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region QueryText変更通知プロパティ
        private string _QueryText;

        public string QueryText
        {
            get
            { return _QueryText; }
            set
            {
                if (_QueryText == value)
                    return;
                _QueryText = value;
                Source.Query = new Kbtter3.Query.Kbtter3Query(value);
                RaisePropertyChanged();
            }
        }
        #endregion


        #region EditCommand
        private ViewModelCommand _EditCommand;

        public ViewModelCommand EditCommand
        {
            get
            {
                if (_EditCommand == null)
                {
                    _EditCommand = new ViewModelCommand(Edit);
                }
                return _EditCommand;
            }
        }

        public void Edit()
        {
            Messenger.Raise(new TransitionMessage(new StatusTimelineEditWindowViewModel { EditingTarget = this }, "StatusTimelineEdit"));
        }
        #endregion


        #region DeleteTimelineCommand
        private ViewModelCommand _DeleteTimelineCommand;

        public ViewModelCommand DeleteTimelineCommand
        {
            get
            {
                if (_DeleteTimelineCommand == null)
                {
                    _DeleteTimelineCommand = new ViewModelCommand(DeleteTimeline);
                }
                return _DeleteTimelineCommand;
            }
        }

        public void DeleteTimeline()
        {
            Kbtter.Instance.StatusTimelines.Remove(Source);
        }
        #endregion

    }
}
