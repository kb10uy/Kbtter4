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
using Kbtter3.Query;

namespace Kbtter4.ViewModels
{
    public class StatusTimelineEditWindowViewModel : ViewModel, IDataErrorInfo
    {

        public void Initialize()
        {
            Name = EditingTarget.Name;
            QueryText = EditingTarget.QueryText;
        }


        #region EditingTarget変更通知プロパティ
        private StatusTimelineViewModel _EditingTarget;

        public StatusTimelineViewModel EditingTarget
        {
            get
            { return _EditingTarget; }
            set
            {
                if (_EditingTarget == value)
                    return;
                _EditingTarget = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region UpdateCommand
        private ViewModelCommand _UpdateCommand;

        public ViewModelCommand UpdateCommand
        {
            get
            {
                if (_UpdateCommand == null)
                {
                    _UpdateCommand = new ViewModelCommand(Update, CanUpdate);
                }
                return _UpdateCommand;
            }
        }

        public bool CanUpdate()
        {
            return this["QueryText"] == null;
        }

        public void Update()
        {
            EditingTarget.Name = Name;
            EditingTarget.QueryText = QueryText;
            Updated = true;
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
                try
                {
                    var q = new Kbtter3Query(value);
                    errors["QueryText"] = null;
                }
                catch
                {
                    errors["QueryText"] = "クエリが不正です";
                }
                UpdateCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Updated { get; private set; }

        #region エラー
        Dictionary<string, string> errors = new Dictionary<string, string>();

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get { return errors.ContainsKey(columnName) ? errors[columnName] : null; }
        }
        #endregion
    }
}
