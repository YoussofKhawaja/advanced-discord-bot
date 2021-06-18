using DiscordClient;
using UIControls.Core;

namespace UIControls.ViewModel
{
    public class ConfigViewModel : ObservableObject
    {
        private string token;

        public string Token
        {
            get { return token; }
            set
            {
                token = Bot.AppData.Token = value;
                OnPropertyChanged();
            }
        }

        private string prefix;

        public string Prefix
        {
            get { return prefix; }
            set
            {
                prefix = Bot.AppData.Prefix = value;
                OnPropertyChanged();
            }
        }

        private string gameStatus;

        public string GameStatus
        {
            get { return gameStatus; }
            set
            {
                gameStatus = Bot.AppData.GameStatus = value;
                OnPropertyChanged();
            }
        }
        public ConfigViewModel()
        {
            Token = Bot.AppData.Token;
            Prefix = Bot.AppData.Prefix;
            GameStatus = Bot.AppData.GameStatus;
        }
    }
}
