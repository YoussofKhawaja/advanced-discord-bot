using System;
using System.Threading;
using System.Windows;
using UIControls;

namespace ADB
{
    public partial class App : Application
    {
        private Mutex myMutex;

        public static MainWindow mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region mutex
            Mutex();
            #endregion

            #region init app
            //window
            mainWindow = new MainWindow();
            mainWindow.Show();
            #endregion
        }
        private void Mutex()
        {
            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "Advanced Discord Bot", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                Console.WriteLine("Already an instance is running...");
                Current.Shutdown();
            }
        }
    }
}