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
    public class StatusViewModel : ViewModel
    {

        public Kbtter4Status SourceStatus { get; private set; }

        public void Initialize()
        {
        }

        public StatusViewModel(Kbtter4Status st)
        {
            SourceStatus = st;
            User = new UserViewModel(st.User);
            Text = SourceStatus.Text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
            OnelineText = Text
                .Replace("\r", " ")
                .Replace("\n", " ");

        }


        #region User変更通知プロパティ
        private UserViewModel _User;

        public UserViewModel User
        {
            get
            { return _User; }
            set
            {
                if (_User == value)
                    return;
                _User = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Text変更通知プロパティ
        private string _Text = "";

        public string Text
        {
            get
            { return _Text; }
            set
            {
                if (_Text == value)
                    return;
                _Text = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region OnelineText変更通知プロパティ
        private string _OnelineText = "";

        public string OnelineText
        {
            get
            { return _OnelineText; }
            set
            {
                if (_OnelineText == value)
                    return;
                _OnelineText = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
