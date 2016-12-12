using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Remote
{
    public class ServiceInfo : INotifyPropertyChanged
    {
        private string name;
        private string machineName;
        private string status;
        private string startupType;

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;
                name = value;
                OnPropertyChanged();
            }
        }

        public string MachineName
        {
            get { return machineName; }
            set
            {
                if (value == machineName) return;
                machineName = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                if (value == status) return;
                status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string StartupType
        {
            get { return startupType; }
            set
            {
                if (value == startupType) return;
                startupType = value;
                OnPropertyChanged();
            }
        }

        public bool? IsRunning
        {
            get
            {
                switch (Status)
                {
                    case "Running":
                        return true;
                    case "Stopped":
                        return false;
                    default:
                        return null;
                }
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                OnPropertyChanged();
            }
        }

        public string Describe()
        {
            return $"{Name}";
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Status)}: {Status}, {nameof(StartupType)}: {StartupType}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}