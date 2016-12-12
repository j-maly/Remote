using System;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;

namespace Remote
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Exception e = args.Exception;
            var mainWindow = (MainWindow)MainWindow;

            if (mainWindow != null && mainWindow.Model != null)
            {
                mainWindow.ShowMessageAsync("Unhandled Error", e.Message)
                    .ContinueWith(task => mainWindow.Model.Refresh());
            }
            args.Handled = true;
        }
    }
}
