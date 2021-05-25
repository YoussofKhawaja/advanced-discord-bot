using ADB.Config;
using ADB.FilesManager;
using ADB.Handlers;
using ADB.Helpers;
using ADB.Models;
using ADB.Services;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ADB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public NotificationManager notificationManager;
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            notificationManager = new NotificationManager();

            LoadData();
        }

        #region properties

        private bool _isEnabled;

        public new bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        private string _state = "Offline";

        public string State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        private string _servers = "0";

        public string Servers
        {
            get { return _servers; }
            set
            {
                _servers = value;
                OnPropertyChanged("Servers");
            }
        }

        #endregion properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region load data

        //set bot config
        public static AppData appData = new AppData();

        private void LoadData()
        {
            appData = SaveManager.ReadFromXmlFile<AppData>("Config", "ConfigData");

            if (appData != null && !string.IsNullOrEmpty(appData.token))
                OnDataLoaded();
        }

        private void OnDataLoaded()
        {
            AppSettings.appId = BotAppIDTextBox.Text = appData.appId;
            AppSettings.appSecret = BotAppSecretTextBox.Text = appData.appSecret;
            AppSettings.CommandPrefix = BotPrefixTextBox.Text = appData.CommandPrefix;
            AppSettings.token = BotTokenTextBox.Text = appData.token;
        }

        #endregion load data

        private void ConfigBot()
        {
            if (string.IsNullOrEmpty(BotTokenTextBox.Text) || string.IsNullOrWhiteSpace(BotTokenTextBox.Text))
            {
                //set default config
                notificationManager.Show("Notice", "Please configure the bot in the Config tab",  NotificationType.Information, "WindowArea");
            }
            else
            {
                //set text boxes config
                appData.appId = AppSettings.appId = BotAppIDTextBox.Text;
                appData.appSecret = AppSettings.appSecret = BotAppSecretTextBox.Text;
                appData.CommandPrefix = AppSettings.CommandPrefix = BotPrefixTextBox.Text;
                appData.token = AppSettings.token = BotTokenTextBox.Text;

                SaveManager.WriteToXmlFile<AppData>(appData, "Config", "ConfigData");
            }
        }

        private void LoadAnalytics()
        {
            //load analytics
            Servers = CommandHandler.instance._client.Guilds.Count.ToString();
        }

        //start
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (!DiscordService.botRunning)
            {
                ConfigBot();
                if (string.IsNullOrEmpty(BotTokenTextBox.Text) || string.IsNullOrWhiteSpace(BotTokenTextBox.Text))
                    return;

                btn.Background = Brushes.Gray;
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;

                SetupBot.InitBot();

                while (!DiscordService.botRunning)
                {
                    await Task.Delay(1000);
                }

                btn.Background = Brushes.Red;
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;

                Console.WriteLine("Analytics Load");
                LoadAnalytics();

                State = "Online";
            }
            else
            {
                SetupBot.DisconnectBot();

                btn.Background = HexColor.GetColorFromHex("#F25922");
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;

                State = "Offline";
            }
        }

        //restart
        private bool isRestarting;

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!isRestarting)
            {
                isRestarting = true;

                Button btn = (Button)sender;

                btn.Background = Brushes.Gray;

                StartBtn.Background = Brushes.Gray;
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;

                ConfigBot();
                await SetupBot.RestartBot();

                while (!DiscordService.botRunning)
                {
                    await Task.Delay(1000);
                }

                LoadAnalytics();

                StartBtn.Background = Brushes.Red;

                btn.Background = HexColor.GetColorFromHex("#F25922");
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;

                isRestarting = false;
            }
        }

        public void DisplayConsole(string text)
        {
            //Allow access from other thread to UI thread
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                ConsoleTextBox.Text += text + "\n";
                //ConsoleTextBox.ScrollToEnd();
            }));
        }

        //exit app
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        //minimize app
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //move system window
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BotTokenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigBot();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
    }
}