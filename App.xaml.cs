using System.Windows;

namespace MaskNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private NotifyManager notifyManager;
        private void OnStartup(object sender, StartupEventArgs eventArgs)
        {
            notifyManager = NotifyManager.GetInstance();
            notifyManager.StartService();
            //notifyManager.ShowWindow();
        }
    }
}