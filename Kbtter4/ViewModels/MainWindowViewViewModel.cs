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
using System.Diagnostics;

namespace Kbtter4.ViewModels
{
    public class MainWindowViewViewModel : ViewModel
    {
        public void Initialize()
        {
        }

        #region SystemCommands的なやつ

        #region WindowCloseCommand
        private ViewModelCommand _WindowCloseCommand;

        public ViewModelCommand WindowCloseCommand
        {
            get
            {
                if (_WindowCloseCommand == null)
                {
                    _WindowCloseCommand = new ViewModelCommand(WindowClose);
                }
                return _WindowCloseCommand;
            }
        }

        public void WindowClose()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }
        #endregion


        #region WindowMaximizeCommand
        private ViewModelCommand _WindowMaximizeCommand;

        public ViewModelCommand WindowMaximizeCommand
        {
            get
            {
                if (_WindowMaximizeCommand == null)
                {
                    _WindowMaximizeCommand = new ViewModelCommand(WindowMaximize);
                }
                return _WindowMaximizeCommand;
            }
        }

        public void WindowMaximize()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Maximize, "Maximize"));
        }
        #endregion


        #region WindowMinimizeCommand
        private ViewModelCommand _WindowMinimizeCommand;

        public ViewModelCommand WindowMinimizeCommand
        {
            get
            {
                if (_WindowMinimizeCommand == null)
                {
                    _WindowMinimizeCommand = new ViewModelCommand(WindowMinimize);
                }
                return _WindowMinimizeCommand;
            }
        }

        public void WindowMinimize()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Minimize, "Minimize"));
        }
        #endregion


        #region WindowRestoreCommand
        private ViewModelCommand _WindowRestoreCommand;

        public ViewModelCommand WindowRestoreCommand
        {
            get
            {
                if (_WindowRestoreCommand == null)
                {
                    _WindowRestoreCommand = new ViewModelCommand(WindowRestore);
                }
                return _WindowRestoreCommand;
            }
        }

        public void WindowRestore()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Normal, "Restore"));
        }
        #endregion

        #endregion

        #region 画面タブ遷移

        #region IsHomeStatusTimelineVisible変更通知プロパティ
        private bool _IsHomeStatusTimelineVisible;

        public bool IsHomeStatusTimelineVisible
        {
            get
            { return _IsHomeStatusTimelineVisible; }
            set
            {
                if (_IsHomeStatusTimelineVisible == value)
                    return;
                _IsHomeStatusTimelineVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsHomeNotificationTimelineVisible変更通知プロパティ
        private bool _IsHomeNotificationTimelineVisible;

        public bool IsHomeNotificationTimelineVisible
        {
            get
            { return _IsHomeNotificationTimelineVisible; }
            set
            {
                if (_IsHomeNotificationTimelineVisible == value)
                    return;
                _IsHomeNotificationTimelineVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsDirectMessageVisible変更通知プロパティ
        private bool _IsDirectMessageVisible;

        public bool IsDirectMessageVisible
        {
            get
            { return _IsDirectMessageVisible; }
            set
            {
                if (_IsDirectMessageVisible == value)
                    return;
                _IsDirectMessageVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsSearchVisible変更通知プロパティ
        private bool _IsSearchVisible;

        public bool IsSearchVisible
        {
            get
            { return _IsSearchVisible; }
            set
            {
                if (_IsSearchVisible == value)
                    return;
                _IsSearchVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsUserVisible変更通知プロパティ
        private bool _IsUserVisible;

        public bool IsUserVisible
        {
            get
            { return _IsUserVisible; }
            set
            {
                if (_IsUserVisible == value)
                    return;
                _IsUserVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsUserStatusTimelineVisible変更通知プロパティ
        private bool _IsUserStatusTimelineVisible;

        public bool IsUserStatusTimelineVisible
        {
            get
            { return _IsUserStatusTimelineVisible; }
            set
            {
                if (_IsUserStatusTimelineVisible == value)
                    return;
                _IsUserStatusTimelineVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsUserNotificationTimelineVisible変更通知プロパティ
        private bool _IsUserNotificationTimelineVisible;

        public bool IsUserNotificationTimelineVisible
        {
            get
            { return _IsUserNotificationTimelineVisible; }
            set
            {
                if (_IsUserNotificationTimelineVisible == value)
                    return;
                _IsUserNotificationTimelineVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsCommandlineVisible変更通知プロパティ
        private bool _IsCommandlineVisible;

        public bool IsCommandlineVisible
        {
            get
            { return _IsCommandlineVisible; }
            set
            {
                if (_IsCommandlineVisible == value)
                    return;
                _IsCommandlineVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsSettingVisible変更通知プロパティ
        private bool _IsSettingVisible;

        public bool IsSettingVisible
        {
            get
            { return _IsSettingVisible; }
            set
            {
                if (_IsSettingVisible == value)
                    return;
                _IsSettingVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region ChangeToHomeStatusTimelineCommand
        private ViewModelCommand _ChangeToHomeStatusTimelineCommand;

        public ViewModelCommand ChangeToHomeStatusTimelineCommand
        {
            get
            {
                if (_ChangeToHomeStatusTimelineCommand == null)
                {
                    _ChangeToHomeStatusTimelineCommand = new ViewModelCommand(ChangeToHomeStatusTimeline);
                }
                return _ChangeToHomeStatusTimelineCommand;
            }
        }

        public void ChangeToHomeStatusTimeline()
        {
            IsHomeStatusTimelineVisible = true;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToHomeNotificationTimelineCommand
        private ViewModelCommand _ChangeToHomeNotificationTimelineCommand;

        public ViewModelCommand ChangeToHomeNotificationTimelineCommand
        {
            get
            {
                if (_ChangeToHomeNotificationTimelineCommand == null)
                {
                    _ChangeToHomeNotificationTimelineCommand = new ViewModelCommand(ChangeToHomeNotificationTimeline);
                }
                return _ChangeToHomeNotificationTimelineCommand;
            }
        }

        public void ChangeToHomeNotificationTimeline()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = true;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToDirectMessageCommand
        private ViewModelCommand _ChangeToDirectMessageCommand;

        public ViewModelCommand ChangeToDirectMessageCommand
        {
            get
            {
                if (_ChangeToDirectMessageCommand == null)
                {
                    _ChangeToDirectMessageCommand = new ViewModelCommand(ChangeToDirectMessage);
                }
                return _ChangeToDirectMessageCommand;
            }
        }

        public void ChangeToDirectMessage()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = true;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToSearchCommand
        private ViewModelCommand _ChangeToSearchCommand;

        public ViewModelCommand ChangeToSearchCommand
        {
            get
            {
                if (_ChangeToSearchCommand == null)
                {
                    _ChangeToSearchCommand = new ViewModelCommand(ChangeToSearch);
                }
                return _ChangeToSearchCommand;
            }
        }

        public void ChangeToSearch()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = true;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToUserCommand
        private ViewModelCommand _ChangeToUserCommand;

        public ViewModelCommand ChangeToUserCommand
        {
            get
            {
                if (_ChangeToUserCommand == null)
                {
                    _ChangeToUserCommand = new ViewModelCommand(ChangeToUser);
                }
                return _ChangeToUserCommand;
            }
        }

        public void ChangeToUser()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = true;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToUserStatusTimelineCommand
        private ViewModelCommand _ChangeToUserStatusTimelineCommand;

        public ViewModelCommand ChangeToUserStatusTimelineCommand
        {
            get
            {
                if (_ChangeToUserStatusTimelineCommand == null)
                {
                    _ChangeToUserStatusTimelineCommand = new ViewModelCommand(ChangeToUserStatusTimeline);
                }
                return _ChangeToUserStatusTimelineCommand;
            }
        }

        public void ChangeToUserStatusTimeline()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = true;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToUserNotificationTimelineCommand
        private ViewModelCommand _ChangeToUserNotificationTimelineCommand;

        public ViewModelCommand ChangeToUserNotificationTimelineCommand
        {
            get
            {
                if (_ChangeToUserNotificationTimelineCommand == null)
                {
                    _ChangeToUserNotificationTimelineCommand = new ViewModelCommand(ChangeToUserNotificationTimeline);
                }
                return _ChangeToUserNotificationTimelineCommand;
            }
        }

        public void ChangeToUserNotificationTimeline()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = true;
            IsCommandlineVisible = false;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToCommandlineCommand
        private ViewModelCommand _ChangeToCommandlineCommand;

        public ViewModelCommand ChangeToCommandlineCommand
        {
            get
            {
                if (_ChangeToCommandlineCommand == null)
                {
                    _ChangeToCommandlineCommand = new ViewModelCommand(ChangeToCommandline);
                }
                return _ChangeToCommandlineCommand;
            }
        }

        public void ChangeToCommandline()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = true;
            IsSettingVisible = false;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion


        #region ChangeToSettingCommand
        private ViewModelCommand _ChangeToSettingCommand;

        public ViewModelCommand ChangeToSettingCommand
        {
            get
            {
                if (_ChangeToSettingCommand == null)
                {
                    _ChangeToSettingCommand = new ViewModelCommand(ChangeToSetting);
                }
                return _ChangeToSettingCommand;
            }
        }

        public void ChangeToSetting()
        {
            IsHomeStatusTimelineVisible = false;
            IsHomeNotificationTimelineVisible = false;
            IsDirectMessageVisible = false;
            IsSearchVisible = false;
            IsUserVisible = false;
            IsUserStatusTimelineVisible = false;
            IsUserNotificationTimelineVisible = false;
            IsCommandlineVisible = false;
            IsSettingVisible = true;

            HomeTimeline.IsSelected = IsHomeStatusTimelineVisible;
            HomeNotification.IsSelected = IsHomeNotificationTimelineVisible;
            HomeStatusTimelineUnselected = !IsHomeStatusTimelineVisible;
            HomeNotificationTimelineUnselected = !IsHomeNotificationTimelineVisible;
        }
        #endregion

        #endregion

        #region メニュー要素

        #region IsNewAccountPanelVisible変更通知プロパティ
        private bool _IsNewAccountPanelVisible = false;

        public bool IsNewAccountPanelVisible
        {
            get
            { return _IsNewAccountPanelVisible; }
            set
            {
                if (_IsNewAccountPanelVisible == value)
                    return;
                _IsNewAccountPanelVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsAccountPanelVisible変更通知プロパティ
        private bool _IsAccountPanelVisible;

        public bool IsAccountPanelVisible
        {
            get
            { return _IsAccountPanelVisible; }
            set
            {
                if (_IsAccountPanelVisible == value)
                    return;
                _IsAccountPanelVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion

        #region デフォで開くあれ

        public void OpenInDefault(Uri uri)
        {
            Process.Start(uri.ToString());
        }

        public void OpenInDefault(string f)
        {
            Process.Start(f);
        }

        #endregion

        #region ポップアップ通知

        #region PopupNotificationText変更通知プロパティ
        private string _PopupNotificationText;

        public string PopupNotificationText
        {
            get
            { return _PopupNotificationText; }
            set
            {
                _PopupNotificationText = value;
            }
        }
        #endregion

        public void Notify(string text)
        {
            PopupNotificationText = text;
            RaisePropertyChanged(() => PopupNotificationText);
        }

        #endregion

        #region その他画面要素

        #region IsStatusCreatorExpanded変更通知プロパティ
        private bool _IsStatusCreatorExpanded;

        public bool IsStatusCreatorExpanded
        {
            get
            { return _IsStatusCreatorExpanded; }
            set
            {
                if (_IsStatusCreatorExpanded == value)
                    return;
                _IsStatusCreatorExpanded = value;
                RaisePropertyChanged();
            }
        }
        #endregion


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


        #region HomeNotification変更通知プロパティ
        private NotificationTimelineViewModel _HomeNotification;

        public NotificationTimelineViewModel HomeNotification
        {
            get
            { return _HomeNotification; }
            set
            {
                if (_HomeNotification == value)
                    return;
                _HomeNotification = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Accounts変更通知プロパティ
        private ReadOnlyDispatcherCollection<AccountViewModel> _Accounts;

        public ReadOnlyDispatcherCollection<AccountViewModel> Accounts
        {
            get
            { return _Accounts; }
            set
            {
                if (_Accounts == value)
                    return;
                _Accounts = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeStatusTimelineUnselected変更通知プロパティ
        private bool _HomeStatusTimelineUnselected;

        public bool HomeStatusTimelineUnselected
        {
            get
            { return _HomeStatusTimelineUnselected; }
            set
            {
                if (_HomeStatusTimelineUnselected == value)
                    return;
                _HomeStatusTimelineUnselected = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region HomeNotificationTimelineUnselected変更通知プロパティ
        private bool _HomeNotificationTimelineUnselected;

        public bool HomeNotificationTimelineUnselected
        {
            get
            { return _HomeNotificationTimelineUnselected; }
            set
            {
                if (_HomeNotificationTimelineUnselected == value)
                    return;
                _HomeNotificationTimelineUnselected = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region DirectMessageTimelines変更通知プロパティ
        private ReadOnlyDispatcherCollection<DirectMessageTimelineViewModel> _DirectMessageTimelines;

        public ReadOnlyDispatcherCollection<DirectMessageTimelineViewModel> DirectMessageTimelines
        {
            get
            { return _DirectMessageTimelines; }
            set
            {
                if (_DirectMessageTimelines == value)
                    return;
                _DirectMessageTimelines = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Users変更通知プロパティ
        private ReadOnlyDispatcherCollection<UserViewModel> _Users;

        public ReadOnlyDispatcherCollection<UserViewModel> Users
        {
            get
            { return _Users; }
            set
            {
                if (_Users == value)
                    return;
                _Users = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion
    }
}
