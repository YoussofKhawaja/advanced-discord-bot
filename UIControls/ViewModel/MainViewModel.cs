using DiscordClient;
using Notification.Wpf;
using UIControls.Core;

namespace UIControls.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region command
        public RelayCommand HomeViewCommand { get; set; }

        public RelayCommand ConfigViewCommand { get; set; }

        public RelayCommand LogViewCommand { get; set; }
        #endregion
        #region viewmodel
        public HomeViewModel homeViewModel { get; set; }

        public ConfigViewModel configViewModel { get; set; }

        public LogViewModel logViewModel { get; set; }
        #endregion
        #region view
        private object currentView;

        public object CurrentView
        {
            get { return currentView; }
            set
            {
                currentView = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region notification
        public static NotificationManager notificationManager;
        #endregion
        public MainViewModel()
        {
            homeViewModel = new HomeViewModel();
            configViewModel = new ConfigViewModel();
            logViewModel = new LogViewModel();

            CurrentView = homeViewModel;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = homeViewModel;
            });

            ConfigViewCommand = new RelayCommand(o =>
            {
                CurrentView = configViewModel;
            });

            LogViewCommand = new RelayCommand(o =>
            {
                CurrentView = logViewModel;
            });

            notificationManager = new NotificationManager();

            Bot.LoadData();
        }

        public static void Notify(string title, string content, NotificationType notificationtype, string areaname)
        {
            notificationManager.Show(title, content, notificationtype, areaname);
        }
    }
}
