using DiscordClient;
using System.Diagnostics;
using UIControls.Core;
using UIControls.Model;

namespace UIControls.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        public RelayCommand ReleaseNotesCommand { get; set; }

        public RelayCommand StartCommand { get; set; }

        private string activeServers;

        public string ActiveServers
        {
            get { return activeServers; }
            set
            {
                activeServers = value;
                OnPropertyChanged();
            }
        }

        private string playKind;

        public string PlayKind
        {
            get { return playKind; }
            set
            {
                playKind = value;
                OnPropertyChanged();
            }
        }

        private string playBackground;

        public string PlayBackground
        {
            get { return playBackground; }
            set
            {
                playBackground = value;
                OnPropertyChanged();
            }
        }

        public HomeViewModel()
        {
            ReleaseNotesCommand = new RelayCommand(o =>
            {
                Process.Start(new ProcessStartInfo("https://github.com/jihadkhawaja/advanced-discord-bot") { UseShellExecute = true });
            });

            StartCommand = new RelayCommand(async o =>
            {
                LogViewModel.Instance.AddText("Initializing, please wait...");

                if(Bot.IsRunning)
                {
                    PlayKind = HomeModel.PlayKind = "Play";
                    PlayBackground = HomeModel.PlayBackground = "SpringGreen";
                    Bot.Disconnect();
                    LogViewModel.Instance.AddText("Bot offline");
                }
                else
                {
                    PlayKind = HomeModel.PlayKind = "Stop";
                    PlayBackground = HomeModel.PlayBackground = "PaleVioletRed";
                    if(await Bot.Init())
                    {
                        ActiveServers = HomeModel.ActiveServers = Bot.GetTotalBotGuilds();
                        LogViewModel.Instance.AddText("Bot online");
                    }
                    else
                    {
                        PlayKind = HomeModel.PlayKind = "Play";
                        PlayBackground = HomeModel.PlayBackground = "SpringGreen";
                        LogViewModel.Instance.AddText("Failed to initialize, please check the bot configuration");
                    }
                }
            });

            ActiveServers = HomeModel.ActiveServers;
            PlayKind = HomeModel.PlayKind;
            PlayBackground = HomeModel.PlayBackground;
        }
    }
}
