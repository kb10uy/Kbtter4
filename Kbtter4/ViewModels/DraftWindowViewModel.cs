using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Kbtter4.Models;

namespace Kbtter4.ViewModels
{
    public class DraftWindowViewModel : ViewModel
    {
        public Kbtter Kbtter;
        MainWindowViewModel main;

        public DraftWindowViewModel(MainWindowViewModel mw)
        {
            Kbtter = Kbtter.Instance;
            main = mw;
            Drafts = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                Kbtter.AuthenticatedUserDrafts,
                p => new DraftViewModel(p),
                DispatcherHelper.UIDispatcher);
            CompositeDisposable.Add(Drafts);
        }

        public DraftWindowViewModel()
        {

        }

        public void Initialize()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Drafts変更通知プロパティ
        private ReadOnlyDispatcherCollection<DraftViewModel> _Drafts;

        public ReadOnlyDispatcherCollection<DraftViewModel> Drafts
        {
            get
            { return _Drafts; }
            set
            {
                if (_Drafts == value)
                    return;
                _Drafts = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region SelectedDraft変更通知プロパティ
        private DraftViewModel _SelectedDraft;

        public DraftViewModel SelectedDraft
        {
            get
            { return _SelectedDraft; }
            set
            {
                if (_SelectedDraft == value)
                    return;
                _SelectedDraft = value;
                RaisePropertyChanged();
                SendDraftCommand.RaiseCanExecuteChanged();
                SendAndDeleteDraftCommand.RaiseCanExecuteChanged();
                DeleteDraftCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SendDraftCommand
        private ViewModelCommand _SendDraftCommand;

        public ViewModelCommand SendDraftCommand
        {
            get
            {
                if (_SendDraftCommand == null)
                {
                    _SendDraftCommand = new ViewModelCommand(SendDraft, CanSendDraft);
                }
                return _SendDraftCommand;
            }
        }

        public bool CanSendDraft()
        {
            return SelectedDraft != null;
        }

        public void SendDraft()
        {
            main.UpdateStatusText = SelectedDraft.Text;
            main.IsReplying = SelectedDraft.IsReply;
            if (SelectedDraft.IsReply) main.ReplyingStatus = SelectedDraft.CreateVirtualStatusViewModel(main);
        }
        #endregion


        #region SendAndDeleteDraftCommand
        private ViewModelCommand _SendAndDeleteDraftCommand;

        public ViewModelCommand SendAndDeleteDraftCommand
        {
            get
            {
                if (_SendAndDeleteDraftCommand == null)
                {
                    _SendAndDeleteDraftCommand = new ViewModelCommand(SendAndDeleteDraft, CanSendAndDeleteDraft);
                }
                return _SendAndDeleteDraftCommand;
            }
        }

        public bool CanSendAndDeleteDraft()
        {
            return SelectedDraft != null;
        }

        public void SendAndDeleteDraft()
        {
            main.UpdateStatusText = SelectedDraft.Text;
            main.IsReplying = SelectedDraft.IsReply;
            if (SelectedDraft.IsReply) main.ReplyingStatus = SelectedDraft.CreateVirtualStatusViewModel(main);
            SelectedDraft.Delete();
        }
        #endregion


        #region DeleteDraftCommand
        private ViewModelCommand _DeleteDraftCommand;

        public ViewModelCommand DeleteDraftCommand
        {
            get
            {
                if (_DeleteDraftCommand == null)
                {
                    _DeleteDraftCommand = new ViewModelCommand(DeleteDraft, CanDeleteDraft);
                }
                return _DeleteDraftCommand;
            }
        }

        public bool CanDeleteDraft()
        {
            return SelectedDraft != null;
        }

        public void DeleteDraft()
        {
            SelectedDraft.Delete();
        }
        #endregion


        #region ReceiveDraftCommand
        private ViewModelCommand _ReceiveDraftCommand;

        public ViewModelCommand ReceiveDraftCommand
        {
            get
            {
                if (_ReceiveDraftCommand == null)
                {
                    _ReceiveDraftCommand = new ViewModelCommand(ReceiveDraft);
                }
                return _ReceiveDraftCommand;
            }
        }

        public void ReceiveDraft()
        {
            Kbtter.AuthenticatedUserDrafts.Add(new Kbtter4Draft(main.UpdateStatusText, DateTime.Now, main.IsReplying, main.IsReplying ? main.ReplyingStatus.SourceStatus : null));
            main.UpdateStatusText = "";
            main.IsReplying = false;
        }
        #endregion

    }
}
