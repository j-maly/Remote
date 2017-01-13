using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Remote
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Current.Dispatcher.Invoke(() => ShowErrorDialog(args.Exception.InnerException ?? args.Exception));
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            ShowErrorDialog(args.Exception);
            args.Handled = true;
        }

        private void ShowErrorDialog(Exception e)
        {
            StringBuilder recursiveMessage = new StringBuilder();
            while (e != null)
            {
                recursiveMessage.AppendLine(e.Message);
                recursiveMessage.AppendLine();
                e = e.InnerException;
            }

            var mainWindow = (MainWindow) MainWindow;

            if (mainWindow != null && mainWindow.Model != null)
            {
                System.Windows.MessageBox.Show("Unhandled Error", recursiveMessage.ToString());
            }
        }
    }
}
