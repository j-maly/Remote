using System;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Remote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ViewModel Model { get; set; }

        public DispatcherTimer Timer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Config config = (Config) ConfigurationManager.GetSection("RemoteConfig");
            Model = new ViewModel(config);
            DataContext = Model;

            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Model.TimerRefreshInterval)
            };
            Timer.Tick += (sender, args) => Model.Refresh();
            Timer.Start();
        }

        private void TextBox_ScrollToEnd(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox)?.ScrollToEnd();
        }
    }
}
