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
    public class TegakiWindowViewModel : ViewModel
    {
        MainWindowViewModel main;

        public TegakiWindowViewModel(MainWindowViewModel mw)
        {
            main = mw;
            PenThickness = main.View.SettingInstance.Miscellaneous.TegakiPenThickness;
        }

        public void Initialize()
        {
        }


        public void AddToMediaList(string path)
        {
            main.AddMedia(new OpeningFileSelectionMessage { Response = new[] { path } });
        }


        #region PenThickness変更通知プロパティ
        private double _PenThickness;

        public double PenThickness
        {
            get
            { return _PenThickness; }
            set
            { 
                if (_PenThickness == value)
                    return;
                _PenThickness = value;
                main.View.SettingInstance.Miscellaneous.TegakiPenThickness = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
