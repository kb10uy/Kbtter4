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
    public sealed class MainWindowViewModel : ViewModel
    {
        PropertyChangedEventListener listener;

        Kbtter Kbtter;

        public MainWindowViewViewModel View { get; private set; }

        public MainWindowViewModel()
        {
            View = new MainWindowViewViewModel();
        }

        public void Initialize()
        {
            Kbtter = Kbtter.Instance;

            Kbtter.Initialize();
            HomeTimeline = new StatusTimelineViewModel(Kbtter.HomeStatusTimeline);
            LoginUser = new UserViewModel(Kbtter.AuthenticatedUser);

            InitializeEventListeners();

            View.ChangeToHomeStatusTimeline();
        }

        private void InitializeEventListeners()
        {
            listener = new PropertyChangedEventListener(Kbtter);
            CompositeDisposable.Add(listener);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CompositeDisposable.Dispose();
        }

        #region LoginUser変更通知プロパティ
        private UserViewModel _LoginUser;

        public UserViewModel LoginUser
        {
            get
            { return _LoginUser; }
            set
            {
                if (_LoginUser == value)
                    return;
                _LoginUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeTimeline変更通知プロパティ
        private StatusTimelineViewModel _HomeTimeline;

        public StatusTimelineViewModel HomeTimeline
        {
            get
            { return _HomeTimeline; }
            set
            {
                if (_HomeTimeline == value)
                    return;
                _HomeTimeline = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
