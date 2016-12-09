using System;
using System.Windows;
using System.Windows.Threading;

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
            MessageBox.Show(e.Message, "Unhandled exception");
            args.Handled = true;

            var mainWindow = (MainWindow)MainWindow;
            if (mainWindow != null && mainWindow.Model != null)
            {
                mainWindow.Model.Refresh();
            }
        }
    }
}
