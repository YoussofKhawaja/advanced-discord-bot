using System;
using System.Threading;
using System.Windows;

namespace ADB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow mainWindow;

        private Mutex myMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "Advanced Discord Bot", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                Console.WriteLine("Already an instance is running...");
                App.Current.Shutdown();
            }

            mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}