using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
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
            Medias = new ObservableSynchronizedCollection<MainWindowSendingMediaViewModel>();
        }

        public void Initialize()
        {
            Kbtter = Kbtter.Instance;


            HomeTimeline = new StatusTimelineViewModel(this, Kbtter.HomeStatusTimeline);
            LoginUser = new UserViewModel(Kbtter.AuthenticatedUser);
            Accounts = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.Accounts,
                p => new AccountViewModel(p),
                DispatcherHelper.UIDispatcher);

            Kbtter.Initialize();

            InitializeEventListeners();
            View.ChangeToHomeStatusTimeline();
            View.IsAccountPanelVisible = true;
        }

        private void InitializeEventListeners()
        {
            listener = new PropertyChangedEventListener(Kbtter);
            CompositeDisposable.Add(listener);

            listener.Add("AuthenticatedUser", (s, e) =>
            {
                LoginUser.Dispose();
                LoginUser = new UserViewModel(Kbtter.AuthenticatedUser);
                UpdateStatusCommand.RaiseCanExecuteChanged();
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

        public async void Login()
        {
            var res = await Kbtter.Authenticate(SelectedAccount.SourceAccount);

            if (res == "")
            {
                View.Notify("ログインに成功しました。");
                View.IsAccountPanelVisible = false;
            }
            else
            {
                View.Notify("ログインに失敗しました : " + res);
            }
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


        #region ﾂｲｰﾖ送信

        #region UpdateStatusTextLength変更通知プロパティ
        private int _UpdateStatusTextLength = 140;

        public int UpdateStatusTextLength
        {
            get
            {
                var s = _UpdateStatusText;
                s = s.Replace("https://", " http://");
                s = Regex.Replace(s, "https?://(([\\w]|[^ -~])+(([\\w\\-]|[^ -~])+([\\w]|[^ -~]))?\\.)+(aero|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sk|sl|sm|sn|so|sr|ss|st|su|sv|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|za|zm|zw)(?![\\w])(/([\\w\\.\\-\\$&%/:=#~!]*\\??[\\w\\.\\-\\$&%/:=#~!]*[\\w\\-\\$/#])?)?", "                      ");
                s = s.Replace("\r", "");
                if (HasMedia) s += "there is a imageaddress";
                _UpdateStatusTextLength = 140 - s.Length;
                return _UpdateStatusTextLength;
            }
        }
        #endregion


        #region UpdateStatusText変更通知プロパティ
        private string _UpdateStatusText = "";

        public string UpdateStatusText
        {
            get
            { return _UpdateStatusText; }
            set
            {
                if (_UpdateStatusText == value)
                    return;
                _UpdateStatusText = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => UpdateStatusTextLength);
                UpdateStatusCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region HasMedia変更通知プロパティ
        private bool _HasMedia;

        public bool HasMedia
        {
            get
            { return Medias.Count != 0; }
        }
        #endregion


        #region Medias変更通知プロパティ
        private ObservableSynchronizedCollection<MainWindowSendingMediaViewModel> _Medias;

        public ObservableSynchronizedCollection<MainWindowSendingMediaViewModel> Medias
        {
            get
            { return _Medias; }
            set
            {
                if (_Medias == value)
                    return;
                _Medias = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region SelectedMedia変更通知プロパティ
        private MainWindowSendingMediaViewModel _SelectedMedia;

        public MainWindowSendingMediaViewModel SelectedMedia
        {
            get
            { return _SelectedMedia; }
            set
            {
                if (_SelectedMedia == value)
                    return;
                _SelectedMedia = value;
                RaisePropertyChanged();
                RemoveMediaCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region AddMediaCommand
        private ListenerCommand<OpeningFileSelectionMessage> _AddMediaCommand;

        public ListenerCommand<OpeningFileSelectionMessage> AddMediaCommand
        {
            get
            {
                if (_AddMediaCommand == null)
                {
                    _AddMediaCommand = new ListenerCommand<OpeningFileSelectionMessage>(AddMedia);
                }
                return _AddMediaCommand;
            }
        }

        public void AddMedia(OpeningFileSelectionMessage parameter)
        {
            foreach (var i in parameter.Response)
            {
                if (Medias.Count < 4) Medias.Add(new MainWindowSendingMediaViewModel(i));
            }
            UpdateStatusCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(() => HasMedia);
            RaisePropertyChanged(() => UpdateStatusTextLength);
        }
        #endregion


        #region RemoveMediaCommand
        private ViewModelCommand _RemoveMediaCommand;

        public ViewModelCommand RemoveMediaCommand
        {
            get
            {
                if (_RemoveMediaCommand == null)
                {
                    _RemoveMediaCommand = new ViewModelCommand(RemoveMedia, CanRemoveMedia);
                }
                return _RemoveMediaCommand;
            }
        }

        public bool CanRemoveMedia()
        {
            return SelectedMedia != null;
        }

        public void RemoveMedia()
        {
            SelectedMedia.Dispose();
            Medias.Remove(SelectedMedia);
            UpdateStatusCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(() => HasMedia);
            RaisePropertyChanged(() => UpdateStatusTextLength);
        }
        #endregion


        #region UpdateStatusCommand
        private ViewModelCommand _UpdateStatusCommand;

        public ViewModelCommand UpdateStatusCommand
        {
            get
            {
                if (_UpdateStatusCommand == null)
                {
                    _UpdateStatusCommand = new ViewModelCommand(UpdateStatus, CanUpdateStatus);
                }
                return _UpdateStatusCommand;
            }
        }

        bool taken = false;
        public bool CanUpdateStatus()
        {
            return
                LoginUser != null && !string.IsNullOrEmpty(LoginUser.ScreenName) &&
                140 > UpdateStatusTextLength && 0 <= UpdateStatusTextLength &&
                !taken;
        }

        public async void UpdateStatus()
        {
            taken = true;
            UpdateStatusCommand.RaiseCanExecuteChanged();

            var opt = new Dictionary<string, object>();
            opt["status"] = UpdateStatusText;
            if (IsReplying) opt["in_reply_to_status_id"] = ReplyingStatus.SourceStatus.Id;

            try
            {
                if (HasMedia)
                {
                    var iil = new List<long>();
                    foreach (var i in Medias) iil.Add((await Kbtter.Token.Media.UploadAsync(media => i.MediaStream)).MediaId);
                    opt["media_ids"] = iil;
                }

                await Kbtter.Token.Statuses.UpdateAsync(opt);
                View.Notify("ツイート送信に成功しました。");
            }
            catch (Exception te)
            {
                View.Notify("ツイート送信中にエラーが発生しました : " + te.Message);
            }


            foreach (var i in Medias) i.Dispose();
            Medias.Clear();
            RaisePropertyChanged(() => HasMedia);
            IsReplying = false;

            taken = false;
            UpdateStatusText = "";
            UpdateStatusCommand.RaiseCanExecuteChanged();
            AddMediaCommand.RaiseCanExecuteChanged();
            RemoveMediaCommand.RaiseCanExecuteChanged();
        }
        #endregion


        #region ReplyingStatus変更通知プロパティ
        private StatusViewModel _ReplyingStatus;

        public StatusViewModel ReplyingStatus
        {
            get
            { return _ReplyingStatus; }
            set
            {
                if (_ReplyingStatus == value)
                    return;
                _ReplyingStatus = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsReplying変更通知プロパティ
        private bool _IsReplying;

        public bool IsReplying
        {
            get
            { return _IsReplying; }
            set
            {
                if (_IsReplying == value)
                    return;
                _IsReplying = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion
    }
}
