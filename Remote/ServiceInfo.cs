namespace Remote
{
    public class ServiceInfo
    {
        public string Name { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; }
        public string StartupType { get; set; }

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
        }

        public string Describe()
        {
            return $"{Name}";
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Status)}: {Status}, {nameof(StartupType)}: {StartupType}";
        }
    }
}