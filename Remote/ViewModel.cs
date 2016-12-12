using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.Practices.Prism.Commands;
using Remote.Utils;

namespace Remote
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region dependencies 
        public ServiceExplorer ServiceExplorer { get; set; }

        public Config Config { get; }

        public Logging Logging { get; set; }
        #endregion 

        #region commands 
        public ICommand StartServiceCommand { get; private set; }
        public ICommand StopServiceCommand { get; private set; }
        public ICommand StartStopServiceCommand { get; private set; }
        public ICommand ChangeStartupTypeCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand OpenServiceLogCommand { get; private set; }
        #endregion

        #region fields

        private bool isBusy;
        private ICollectionView displayedServicesView;
        private string namePattern;
        private Regex namePatternRegex;
        private ICollectionView logDisplay;
        private MachineElement machine;
        private int timerRefreshInterval = 60;
        private ServiceInfo selectedService;

        #endregion

        // for designer only
        public ViewModel() : this(
            new Config
            {
                Machines = new MachineElementCollection
                {
                    new MachineElement
                    {
                        MachineName = "localhost",
                        Label = "localhost"
                    }
                }
            })
        { }

        public ViewModel(Config config)
        {
            ServiceExplorer = new ServiceExplorer();
            StartServiceCommand = DelegateCommand.FromAsyncHandler(StartService);
            StopServiceCommand = DelegateCommand.FromAsyncHandler(StopService);
            StartStopServiceCommand = DelegateCommand.FromAsyncHandler(StartStopService);
            ChangeStartupTypeCommand = DelegateCommand.FromAsyncHandler(ChangeStartupType);
            OpenServiceLogCommand = DelegateCommand.FromAsyncHandler(OpenServiceLog);
            RefreshCommand = DelegateCommand.FromAsyncHandler(Refresh);

            Config = config;
            Logging = new Logging(Config);
            Logging.GlobalLogFileChanged += LoggingOnGlobalLogFileChanged;

            Machine = Config.Machines.FirstOrDefault();
        }

        #region UI view model properties

        public ICollectionView DisplayedServicesView
        {
            get { return displayedServicesView; }
            set
            {
                if (Equals(value, displayedServicesView)) return;
                displayedServicesView = value;
                OnPropertyChanged();
            }
        }

        public ServiceInfo SelectedService
        {
            get { return selectedService; }
            private set
            {
                if (Equals(value, selectedService)) return;
                selectedService = value;
                OnPropertyChanged();
                LogDisplay.Refresh();
                OnPropertyChanged(nameof(LogDisplay));
            }
        }

        public MachineElement Machine
        {
            get { return machine; }
            set
            {
                if (Equals(value, machine)) return;
                machine = value;
                OnPropertyChanged();
                Refresh();
            }
        }

        public ICollectionView LogDisplay
        {
            get { return logDisplay; }
            set
            {
                if (Equals(value, logDisplay)) return;
                logDisplay = value;
                OnPropertyChanged();
            }
        }

        public int? NumberOfLogLinesShown { get; set; }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (value == isBusy) return;
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public string NamePattern
        {
            get { return namePattern; }
            set
            {
                if (value == namePattern) return;
                namePattern = value;
                namePatternRegex = IOUtils.WildcardToRegex(NamePattern);
                OnPropertyChanged();
                displayedServicesView.Refresh();
            }
        }

        private bool FilterByNamePattern(ServiceInfo serviceInfo)
        {
            return serviceInfo != null &&
                   (namePatternRegex == null || namePatternRegex.IsMatch(serviceInfo.Name));
        }

        private bool FilterBySelectedService(string line)
        {
            if (SelectedService == null)
                return true;
            else if (line.Contains(SelectedService.Name))
                return true;
            else
                return false;
        }

        public int TimerRefreshInterval
        {
            get { return timerRefreshInterval; }
            set
            {
                if (value == timerRefreshInterval) return;
                timerRefreshInterval = value;
                OnPropertyChanged();
            }
        }

        #endregion 

        #region service operation 

        public async Task StartService()
        {
            if (SelectedService.StartupType == "Disabled")
            {
                throw new InvalidOperationException("Cannot start disabled service. ");
            }
            using (RunInBackground())
            {
                await ServiceExplorer.StartService(SelectedService);
                Logging.AppendGlobalLineFormat("Service started: {0}", SelectedService.Describe());
                await Refresh();
            }
        }

        public async Task StopService()
        {
            using (RunInBackground())
            {
                await ServiceExplorer.StopService(SelectedService);
                Logging.AppendGlobalLineFormat("Service stopped: {0}", SelectedService.Describe());
                await Refresh();
            }
        }

        public async Task StartStopService()
        {
            using (RunInBackground())
            {
                if (SelectedService.IsRunning == true)
                {
                    await StopService();
                }
                else if (SelectedService.IsRunning == false)
                {
                    await StartService();
                }
            }
        }

        public async Task ChangeStartupType()
        {
            using (RunInBackground())
            {
                await ServiceExplorer.ChangeStartupType(SelectedService, SelectedService.StartupType);
                Logging.AppendGlobalLineFormat("Set {0} startup type to {1}", SelectedService.Describe(), SelectedService.StartupType);
                await Refresh();
            }
        }

        private async Task OpenServiceLog()
        {
            string latestFile;
            using (RunInBackground())
            {
                await Task.Run(() =>
                {
                    var logDir = new DirectoryInfo(Machine.LogFolder);
                    var files = logDir.GetFiles($"*{SelectedService.Name}*");
                    latestFile = files.OrderBy(f => f.LastWriteTime).LastOrDefault()?.FullName;
                    if (latestFile != null)
                    {
                        ProcessStartInfo ps = new ProcessStartInfo(Config.LogViewer, latestFile);
                        Process.Start(ps);
                    }
                    else
                    {
                        throw new FileNotFoundException($"No log file found in folder '{Machine.LogFolder}' for service '{SelectedService.Describe()}'.");
                    }
                });
            }
        }

        public async Task Refresh()
        {
            if (Machine == null)
            {
                return;
            }
            using (RunInBackground())
            {
                List<ServiceInfo> displayedServices = await ServiceExplorer.GetServices(Machine.MachineName);
                DisplayedServicesView = CollectionViewSource.GetDefaultView(displayedServices);
                DisplayedServicesView.Filter = o => FilterByNamePattern((ServiceInfo)o);
                LoggingOnGlobalLogFileChanged(null, null);
            }
        }

        #endregion

        #region event handling

        private void LoggingOnGlobalLogFileChanged(object sender, EventArgs e)
        {
            var logLines = Logging.GetLogTail(NumberOfLogLinesShown);
            LogDisplay = CollectionViewSource.GetDefaultView(logLines);
            LogDisplay.Filter = l => FilterBySelectedService((string)l);
            LogDisplay.Refresh();
            OnPropertyChanged(nameof(LogDisplay));
        }


        #endregion

        #region notify property changed 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region backgorund operation support

        private BackgroundOperation RunInBackground()
        {
            return new BackgroundOperation(this);
        }

        public class BackgroundOperation : IDisposable
        {
            private readonly ViewModel model;

            public BackgroundOperation(ViewModel model)
            {
                this.model = model;
                model.IsBusy = true;
            }

            public void Dispose()
            {
                model.IsBusy = false;
            }
        }

        #endregion
    }
}