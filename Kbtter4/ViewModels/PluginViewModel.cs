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
using Kbtter4.Models.Plugin;

namespace Kbtter4.ViewModels
{
    public class PluginViewModel : ViewModel
    {
        Kbtter4Plugin p;
        public PluginViewModel(Kbtter4Plugin p)
        {
            this.p = p;
            Name = p.Name;
        }

        public void Initialize()
        {
        }


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
                RaisePropertyChanged();
            }
        }
        #endregion


        #region CallInitializeCommand
        private ViewModelCommand _CallInitializeCommand;

        public ViewModelCommand CallInitializeCommand
        {
            get
            {
                if (_CallInitializeCommand == null)
                {
                    _CallInitializeCommand = new ViewModelCommand(CallInitialize);
                }
                return _CallInitializeCommand;
            }
        }

        public void CallInitialize()
        {
            p.Initialize();
        }
        #endregion


        #region CallDisposeCommand
        private ViewModelCommand _CallDisposeCommand;

        public ViewModelCommand CallDisposeCommand
        {
            get
            {
                if (_CallDisposeCommand == null)
                {
                    _CallDisposeCommand = new ViewModelCommand(CallDispose);
                }
                return _CallDisposeCommand;
            }
        }

        public void CallDispose()
        {
            p.Dispose();
        }
        #endregion


        #region CallOnLoginCommand
        private ViewModelCommand _CallOnLoginCommand;

        public ViewModelCommand CallOnLoginCommand
        {
            get
            {
                if (_CallOnLoginCommand == null)
                {
                    _CallOnLoginCommand = new ViewModelCommand(CallOnLogin);
                }
                return _CallOnLoginCommand;
            }
        }

        public void CallOnLogin()
        {
            p.OnLogin(Kbtter.Instance.AuthenticatedUser);
        }
        #endregion


        #region CallOnLogoutCommand
        private ViewModelCommand _CallOnLogoutCommand;

        public ViewModelCommand CallOnLogoutCommand
        {
            get
            {
                if (_CallOnLogoutCommand == null)
                {
                    _CallOnLogoutCommand = new ViewModelCommand(CallOnLogout);
                }
                return _CallOnLogoutCommand;
            }
        }

        public void CallOnLogout()
        {
            p.OnLogout(Kbtter.Instance.AuthenticatedUser);
        }
        #endregion


        #region CallStartStreamingCommand
        private ViewModelCommand _CallStartStreamingCommand;

        public ViewModelCommand CallStartStreamingCommand
        {
            get
            {
                if (_CallStartStreamingCommand == null)
                {
                    _CallStartStreamingCommand = new ViewModelCommand(CallStartStreaming);
                }
                return _CallStartStreamingCommand;
            }
        }

        public void CallStartStreaming()
        {
            p.OnStartStreaming();
        }
        #endregion


        #region CallStopStreamingCommand
        private ViewModelCommand _CallStopStreamingCommand;

        public ViewModelCommand CallStopStreamingCommand
        {
            get
            {
                if (_CallStopStreamingCommand == null)
                {
                    _CallStopStreamingCommand = new ViewModelCommand(CallStopStreaming);
                }
                return _CallStopStreamingCommand;
            }
        }

        public void CallStopStreaming()
        {
            p.OnStopStreaming();
        }
        #endregion

    }
}
