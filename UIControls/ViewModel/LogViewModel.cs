using System.Windows;
using UIControls.Core;
using UIControls.Model;

namespace UIControls.ViewModel
{
    public class LogViewModel : ObservableObject
    {
        public static LogViewModel Instance;

        private Visibility emptyVisibility;

        public Visibility EmptyVisibility
        {
            get { return emptyVisibility; }
            set
            {
                emptyVisibility = value;
                OnPropertyChanged();
            }
        }

        private string log;

        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                OnPropertyChanged();
            }
        }

        public void AddText(string text)
        {
            LogModel.Data += $"> {text}\n";
            Log = LogModel.Data;

            if (string.IsNullOrEmpty(Log) || string.IsNullOrWhiteSpace(Log))
                EmptyVisibility = Visibility.Visible;
            else
                EmptyVisibility = Visibility.Hidden;
        }

        public LogViewModel()
        {
            Instance = this;

            Log = LogModel.Data;

            if (string.IsNullOrEmpty(Log) || string.IsNullOrWhiteSpace(Log))
                EmptyVisibility = Visibility.Visible;
            else
                EmptyVisibility = Visibility.Hidden;
        }
    }
}
