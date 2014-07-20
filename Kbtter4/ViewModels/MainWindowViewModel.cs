using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

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
            Kbtter.Initialize();

            View.SettingInstance = Kbtter.Setting;
            View.HomeTimeline = new StatusTimelineViewModel(this, Kbtter.HomeStatusTimeline);
            View.HomeNotification = new NotificationTimelineViewModel(this, Kbtter.HomeNotificationTimeline);
            View.LoginUser = new UserViewModel(Kbtter.AuthenticatedUser, this);
            View.Accounts = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.Accounts,
                p => new AccountViewModel(p),
                DispatcherHelper.UIDispatcher);
            View.DirectMessageTimelines = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.DirectMessageTimelines,
                p => new DirectMessageTimelineViewModel(this, p),
                DispatcherHelper.UIDispatcher);
            View.Users = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.Users,
                p => new UserViewModel(p, this, true),
                DispatcherHelper.UIDispatcher);
            View.Plugins = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.GlobalPlugins,
                p => new PluginViewModel(p),
                DispatcherHelper.UIDispatcher);

            View.SearchResultStatuses = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.SearchResultStatuses,
                p => new StatusViewModel(this, p),
                DispatcherHelper.UIDispatcher);
            View.SearchResultUsers = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.SearchResultUsers,
                p => new UserViewModel(p, this),
                DispatcherHelper.UIDispatcher);

            View.UserDefinitionTimelines = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.StatusTimelines,
                p => new StatusTimelineViewModel(this, p),
                DispatcherHelper.UIDispatcher);

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
                View.LoginUser.Dispose();
                View.LoginUser = new UserViewModel(Kbtter.AuthenticatedUser, this);
                UpdateStatusCommand.RaiseCanExecuteChanged();
            });

            listener.Add("NotifyText", (s, e) =>
            {
                View.Notify(Kbtter.NotifyText);
            });

            listener.Add("HeadlineText", (s, e) =>
            {
                View.HeadlineText = Kbtter.HeadlineText;
                View.HeadlineUserImage = Kbtter.HeadlineUserImage;
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CompositeDisposable.Dispose();
        }


        #region Extra変更通知プロパティ
        private MainWindowExtraViewModel _Extra = new MainWindowExtraViewModel();

        public MainWindowExtraViewModel Extra
        {
            get
            { return _Extra; }
            set
            {
                if (_Extra == value)
                    return;
                _Extra = value;
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
        bool logintaken = false;

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
            return SelectedAccount != null && !logintaken;
        }

        public async void Login()
        {
            logintaken = true;
            LoginCommand.RaiseCanExecuteChanged();
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
            logintaken = false;
            LoginCommand.RaiseCanExecuteChanged();
            IsLogined = true;
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


        #region IsLogined変更通知プロパティ
        private bool _IsLogined;

        public bool IsLogined
        {
            get
            { return _IsLogined; }
            set
            {
                if (_IsLogined == value)
                    return;
                _IsLogined = value;
                RaisePropertyChanged();
                OpenDraftWindowCommand.RaiseCanExecuteChanged();
                QuickReceiveDraftCommand.RaiseCanExecuteChanged();
                QuickSendDraftCommand.RaiseCanExecuteChanged();
            }
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
                QuickSendDraftCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region HasMedia変更通知プロパティ

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
            if (parameter.Response == null) return;
            foreach (var i in parameter.Response)
            {
                if (Medias.Count < 4) Medias.Add(new MainWindowSendingMediaViewModel(i));
            }
            UpdateStatusCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(() => HasMedia);
            RaisePropertyChanged(() => UpdateStatusTextLength);
        }
        #endregion


        #region MediaDragDropAcceptCommand
        private ListenerCommand<FileDragDropResult> _MediaDragDropAcceptCommand;

        public ListenerCommand<FileDragDropResult> MediaDragDropAcceptCommand
        {
            get
            {
                if (_MediaDragDropAcceptCommand == null)
                {
                    _MediaDragDropAcceptCommand = new ListenerCommand<FileDragDropResult>(MediaDragDropAccept);
                }
                return _MediaDragDropAcceptCommand;
            }
        }

        public void MediaDragDropAccept(FileDragDropResult parameter)
        {
            var files = parameter.Files.Where(p => p.EndsWith(new[] { ".png", "jpg", ".gif" }, StringComparison.OrdinalIgnoreCase));
            foreach (var i in files)
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
                View.LoginUser != null && !string.IsNullOrEmpty(View.LoginUser.ScreenName) &&
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


        #region リプ

        public void SetReplyTo(StatusViewModel st)
        {
            if (View.SettingInstance.Draft.MakeDraftWhenReply && UpdateStatusText != "") Kbtter.AuthenticatedUserDrafts.Add(new Kbtter4Draft(UpdateStatusText, DateTime.Now, IsReplying, IsReplying ? ReplyingStatus.SourceStatus : null));
            ReplyingStatus = st;
            IsReplying = true;

            var s = st.SourceStatus;
            List<string> ru = s.Entities.UserMentions.Select(p => p.ScreenName).ToList();
            if (!ru.Contains(s.User.ScreenName)) ru.Insert(0, s.User.ScreenName);
            if (ru.Count != 1 && ru.Contains(Kbtter.AuthenticatedUser.ScreenName)) ru.Remove(Kbtter.AuthenticatedUser.ScreenName);
            var t = new StringBuilder();
            ru.ForEach(p => t.Append(String.Format("@{0} ", p)));
            UpdateStatusText = t.ToString();

            View.IsStatusCreatorExpanded = true;
        }


        #region CancelReplyCommand
        private ViewModelCommand _CancelReplyCommand;

        public ViewModelCommand CancelReplyCommand
        {
            get
            {
                if (_CancelReplyCommand == null)
                {
                    _CancelReplyCommand = new ViewModelCommand(CancelReply);
                }
                return _CancelReplyCommand;
            }
        }

        public void CancelReply()
        {
            UpdateStatusText = "";
            IsReplying = false;
        }
        #endregion


        #endregion


        #endregion


        #region StartTegakiCommand
        private ViewModelCommand _StartTegakiCommand;

        public ViewModelCommand StartTegakiCommand
        {
            get
            {
                if (_StartTegakiCommand == null)
                {
                    _StartTegakiCommand = new ViewModelCommand(StartTegaki);
                }
                return _StartTegakiCommand;
            }
        }

        public void StartTegaki()
        {
            Messenger.RaiseAsync(new TransitionMessage(new TegakiWindowViewModel(this), "Tegaki"));
        }
        #endregion


        #region NewStatusTimelineCommand
        private ViewModelCommand _NewStatusTimelineCommand;

        public ViewModelCommand NewStatusTimelineCommand
        {
            get
            {
                if (_NewStatusTimelineCommand == null)
                {
                    _NewStatusTimelineCommand = new ViewModelCommand(NewStatusTimeline);
                }
                return _NewStatusTimelineCommand;
            }
        }

        public void NewStatusTimeline()
        {
            var stt = new StatusTimeline(Kbtter.Setting, "true", "新しいやつ");
            var vm = new StatusTimelineEditWindowViewModel { EditingTarget = new StatusTimelineViewModel(this, stt) };
            Messenger.Raise(new TransitionMessage(vm, "StatusTimelineEdit"));
            if (vm.Updated) Kbtter.StatusTimelines.Add(stt);
        }
        #endregion


        #region OpenDraftWindowCommand
        private ViewModelCommand _OpenDraftWindowCommand;

        public ViewModelCommand OpenDraftWindowCommand
        {
            get
            {
                if (_OpenDraftWindowCommand == null)
                {
                    _OpenDraftWindowCommand = new ViewModelCommand(OpenDraftWindow, CanOpenDraftWindow);
                }
                return _OpenDraftWindowCommand;
            }
        }

        public bool CanOpenDraftWindow()
        {
            return IsLogined;
        }

        public void OpenDraftWindow()
        {
            Messenger.Raise(new TransitionMessage(new DraftWindowViewModel(this), "DraftWindow"));
        }
        #endregion


        #region QuickDraft変更通知プロパティ
        private DraftViewModel _QuickDraft;

        public DraftViewModel QuickDraft
        {
            get
            { return _QuickDraft; }
            set
            {
                if (_QuickDraft == value)
                    return;
                _QuickDraft = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region QuickSendDraftCommand
        private ViewModelCommand _QuickSendDraftCommand;

        public ViewModelCommand QuickSendDraftCommand
        {
            get
            {
                if (_QuickSendDraftCommand == null)
                {
                    _QuickSendDraftCommand = new ViewModelCommand(QuickSendDraft, CanQuickSendDraft);
                }
                return _QuickSendDraftCommand;
            }
        }

        public bool CanQuickSendDraft()
        {
            return IsLogined && !string.IsNullOrEmpty(UpdateStatusText);
        }

        public void QuickSendDraft()
        {
            var dr = new Kbtter4Draft(UpdateStatusText, DateTime.Now, IsReplying, IsReplying ? ReplyingStatus.SourceStatus : null);
            QuickDraft = new DraftViewModel(dr);
            QuickReceiveDraftCommand.RaiseCanExecuteChanged();
            IsReplying = false;
            UpdateStatusText = "";
        }
        #endregion


        #region QuickReceiveDraftCommand
        private ViewModelCommand _QuickReceiveDraftCommand;

        public ViewModelCommand QuickReceiveDraftCommand
        {
            get
            {
                if (_QuickReceiveDraftCommand == null)
                {
                    _QuickReceiveDraftCommand = new ViewModelCommand(QuickReceiveDraft, CanQuickReceiveDraft);
                }
                return _QuickReceiveDraftCommand;
            }
        }

        public bool CanQuickReceiveDraft()
        {
            return IsLogined && QuickDraft != null;
        }

        public void QuickReceiveDraft()
        {
            UpdateStatusText = QuickDraft.Text;
            IsReplying = QuickDraft.IsReply;
            ReplyingStatus = QuickDraft.CreateVirtualStatusViewModel(this);
            QuickDraft = null;
            QuickReceiveDraftCommand.RaiseCanExecuteChanged();

        }
        #endregion


        #region DM

        #region NewDirectMessagePartyScreenName変更通知プロパティ
        private string _NewDirectMessagePartyScreenName;

        public string NewDirectMessagePartyScreenName
        {
            get
            { return _NewDirectMessagePartyScreenName; }
            set
            {
                if (_NewDirectMessagePartyScreenName == value)
                    return;
                _NewDirectMessagePartyScreenName = value;
                RaisePropertyChanged();
                AddDirectMessagePartyCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region AddDirectMessagePartyCommand
        private ViewModelCommand _AddDirectMessagePartyCommand;

        public ViewModelCommand AddDirectMessagePartyCommand
        {
            get
            {
                if (_AddDirectMessagePartyCommand == null)
                {
                    _AddDirectMessagePartyCommand = new ViewModelCommand(AddDirectMessageParty, CanAddDirectMessageParty);
                }
                return _AddDirectMessagePartyCommand;
            }
        }

        public bool CanAddDirectMessageParty()
        {
            return !string.IsNullOrEmpty(NewDirectMessagePartyScreenName);
        }

        public async void AddDirectMessageParty()
        {
            try
            {
                var fs = await Kbtter.Token.Friendships.ShowAsync(source_screen_name => Kbtter.AuthenticatedUser.ScreenName, target_screen_name => NewDirectMessagePartyScreenName);
                if (!fs.Target.CanDM ?? false)
                {
                    View.Notify("そのユーザーにはダイレクトメッセージを送れません。");
                    return;
                }
                var pu = await Kbtter.Token.Users.ShowAsync(screen_name => NewDirectMessagePartyScreenName);
                var dmtl = new DirectMessageTimeline(Kbtter.Setting, pu);
                Kbtter.DirectMessageTimelines.Add(dmtl);
                NewDirectMessagePartyScreenName = "";
                View.Notify("ユーザーの追加に成功しました。");
            }
            catch (TwitterException e)
            {
                View.Notify("操作中にエラーが発生しました : " + e.Message);
            }
            catch { }


        }
        #endregion


        #region SelectedParty変更通知プロパティ
        private DirectMessageTimelineViewModel _SelectedParty;

        public DirectMessageTimelineViewModel SelectedParty
        {
            get
            { return _SelectedParty; }
            set
            {
                if (_SelectedParty == value)
                    return;
                _SelectedParty = value;
                RaisePropertyChanged();
                SendDirectMessageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SendingDirectMessageTextLength変更通知プロパティ
        //private int _SendingDirectMessageTextLength;

        public int SendingDirectMessageTextLength
        {
            get
            {
                var s = _SendingDirectMessageText ?? "";
                s = s.Replace("https://", " http://");
                s = Regex.Replace(s, "https?://(([\\w]|[^ -~])+(([\\w\\-]|[^ -~])+([\\w]|[^ -~]))?\\.)+(aero|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sk|sl|sm|sn|so|sr|ss|st|su|sv|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|za|zm|zw)(?![\\w])(/([\\w\\.\\-\\$&%/:=#~!]*\\??[\\w\\.\\-\\$&%/:=#~!]*[\\w\\-\\$/#])?)?", "                      ");
                s = s.Replace("\r", "");
                var ret = 140 - s.Length;
                return ret;
            }
        }
        #endregion


        #region SendingDirectMessageText変更通知プロパティ
        private string _SendingDirectMessageText;

        public string SendingDirectMessageText
        {
            get
            { return _SendingDirectMessageText; }
            set
            {
                if (_SendingDirectMessageText == value)
                    return;
                _SendingDirectMessageText = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => SendingDirectMessageTextLength);
                SendDirectMessageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SendDirectMessageCommand
        private ViewModelCommand _SendDirectMessageCommand;

        public ViewModelCommand SendDirectMessageCommand
        {
            get
            {
                if (_SendDirectMessageCommand == null)
                {
                    _SendDirectMessageCommand = new ViewModelCommand(SendDirectMessage, CanSendDirectMessage);
                }
                return _SendDirectMessageCommand;
            }
        }

        bool dmtaken = false;
        public bool CanSendDirectMessage()
        {
            return
                SelectedParty != null && !string.IsNullOrEmpty(SelectedParty.Party.ScreenName) &&
                140 > SendingDirectMessageTextLength && 0 <= SendingDirectMessageTextLength &&
                !dmtaken;
        }

        public async void SendDirectMessage()
        {
            dmtaken = true;
            SendDirectMessageCommand.RaiseCanExecuteChanged();

            try
            {
                await Kbtter.Token.DirectMessages.NewAsync(screen_name => SelectedParty.Party.ScreenName, text => SendingDirectMessageText);
            }
            catch (TwitterException e)
            {
                View.Notify("ダイレクトメッセージの送信に失敗しました : " + e.Message);
            }
            catch
            {

            }

            dmtaken = false;
            SendingDirectMessageText = "";
            SendDirectMessageCommand.RaiseCanExecuteChanged();
        }
        #endregion


        #endregion


        #region ユーザー画面


        #region SelectedUser変更通知プロパティ
        private UserViewModel _SelectedUser = new UserViewModel { IdString = "" };

        public UserViewModel SelectedUser
        {
            get
            { return _SelectedUser; }
            set
            {
                if (value == null) return;
                if (value == _SelectedUser)
                    return;
                if (_SelectedUser.IdString != value.IdString) value.GetFriendship();
                _SelectedUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #endregion


    }
}
