using System.Collections.Generic;
using System.Configuration;

namespace Remote
{
    public class Config : ConfigurationSection 
    {
        [ConfigurationProperty("sharedLogFile", IsRequired = true)]
        public string SharedLogFile
        {
            get
            {
                return (string) this["sharedLogFile"];
            }
            set
            {
                this["sharedLogFile"] = value;
            }
        }

        [ConfigurationProperty("logViewer", IsRequired = true)]
        public string LogViewer
        {
            get
            {
                return (string)this["logViewer"];
            }
            set
            {
                this["logViewer"] = value;
            }
        }

        [ConfigurationCollection(typeof(MachineElement), AddItemName = "Machine")]
        [ConfigurationProperty("Machines", IsRequired = true)]
        public MachineElementCollection Machines
        {
            get { return (MachineElementCollection)this["Machines"]; }
            set { this["Machines"] = value; }
        }
    }
    
    public class MachineElementCollection : ConfigurationElementCollection, IEnumerable<MachineElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MachineElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MachineElement)element).Label;
        }

        public new MachineElement this[string name]
        {
            get
            {
                return (MachineElement)BaseGet(name);
            }
        }

        IEnumerator<MachineElement> IEnumerable<MachineElement>.GetEnumerator()
        {
            foreach (MachineElement machineElement in this)
            {
                yield return machineElement;
            }
        }

        public void Add(MachineElement machine)
        {
            BaseAdd(machine);
        }
    }

    public class MachineElement : ConfigurationElement
    {
        [ConfigurationProperty("label", IsRequired = true)]
        public string Label
        {
            get
            {
                return (string)this["label"];
            }
            set
            {
                this["label"] = value;
            }
        }

        [ConfigurationProperty("machineName", IsRequired = true)]
        public string MachineName
        {
            get
            {
                return (string)this["machineName"];
            }
            set
            {
                this["machineName"] = value;
            }
        }

        [ConfigurationProperty("logFolder", IsRequired = true)]
        public string LogFolder
        {
            get
            {
                return (string)this["logFolder"];
            }
            set
            {
                this["logFolder"] = value;
            }
        }
    }
}