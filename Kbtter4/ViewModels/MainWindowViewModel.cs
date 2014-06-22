using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

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


            HomeTimeline = new StatusTimelineViewModel(Kbtter.HomeStatusTimeline);
            LoginUser = new UserViewModel(Kbtter.AuthenticatedUser);
            Accounts = ViewModelHelper.CreateReadOnlyDispatcherCollection(Kbtter.Accounts, p => new AccountViewModel(p), DispatcherHelper.UIDispatcher);
            Kbtter.Initialize();

            InitializeEventListeners();
            View.ChangeToHomeStatusTimeline();
        }

        private void InitializeEventListeners()
        {
            listener = new PropertyChangedEventListener(Kbtter);
            CompositeDisposable.Add(listener);

            listener.Add("AuthenticatedUser", (s, e) =>
            {
                LoginUser.Dispose();
                LoginUser = new UserViewModel(Kbtter.AuthenticatedUser);
            });
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


        #region アカウント登録関係
        OAuth.OAuthSession nowsettion;

        public async void StartRegisteringAccount()
        {
            await Task.Run(async () =>
            {
                nowsettion = await Kbtter.CreateOAuthSession();
                View.OpenInDefault(nowsettion.AuthorizeUri);
                View.IsNewAccountPanelVisible = true;
                RegisterAccountCommand.RaiseCanExecuteChanged();
            });
        }

        #region RegisterAccountCommand
        private ViewModelCommand _RegisterAccountCommand;

        public ViewModelCommand RegisterAccountCommand
        {
            get
            {
                if (_RegisterAccountCommand == null)
                {
                    _RegisterAccountCommand = new ViewModelCommand(RegisterAccount, CanRegisterAccount);
                }
                return _RegisterAccountCommand;
            }
        }

        public bool CanRegisterAccount()
        {
            return PinCode.Length == 7 && nowsettion != null;
        }

        public async void RegisterAccount()
        {
            var sc = await Kbtter.RegisterAccount(nowsettion, PinCode);
            if (sc)
            {
                View.Notify("登録に成功しました!");
            }
            else
            {
                View.Notify("登録に失敗しました。PINコードが間違っていませんか?");
            }
            PinCode = "";
            View.IsNewAccountPanelVisible = false;
        }
        #endregion


        #region PinCode変更通知プロパティ
        private string _PinCode = "";

        public string PinCode
        {
            get
            { return _PinCode; }
            set
            {
                if (_PinCode == value)
                    return;
                _PinCode = value;
                RaisePropertyChanged();
                RegisterAccountCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #endregion


        #region 認証

        #region LoginCommand
        private ViewModelCommand _LoginCommand;

        public ViewModelCommand LoginCommand
        {
            get
            {
                if (_LoginCommand == null)
                {
                    _LoginCommand = new ViewModelCommand(Login, CanLogin);
                }
                return _LoginCommand;
            }
        }

        public bool CanLogin()
        {
            return SelectedAccount != null;
        }

        public void Login()
        {
            Kbtter.Authenticate(SelectedAccount.SourceAccount);
        }
        #endregion


        #region SelectedAccount変更通知プロパティ
        private AccountViewModel _SelectedAccount;

        public AccountViewModel SelectedAccount
        {
            get
            { return _SelectedAccount; }
            set
            {
                if (_SelectedAccount == value)
                    return;
                _SelectedAccount = value;
                RaisePropertyChanged();
                LoginCommand.RaiseCanExecuteChanged();
                RemoveAccountCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region RemoveAccountCommand
        private ViewModelCommand _RemoveAccountCommand;

        public ViewModelCommand RemoveAccountCommand
        {
            get
            {
                if (_RemoveAccountCommand == null)
                {
                    _RemoveAccountCommand = new ViewModelCommand(RemoveAccount, CanRemoveAccount);
                }
                return _RemoveAccountCommand;
            }
        }

        public bool CanRemoveAccount()
        {
            return SelectedAccount != null;
        }

        public void RemoveAccount()
        {
            Kbtter.RemoveAccount(SelectedAccount.SourceAccount);
        }
        #endregion


        #endregion
    }
}
