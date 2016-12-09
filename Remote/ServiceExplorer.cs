using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Remote
{
    public class ServiceExplorer
    {
        public Task<List<ServiceInfo>> GetServices(string machineName)
        {
            return Task.Run(() =>
            {
                ServiceController[] serviceControllers = ServiceController.GetServices(machineName);
                List<ServiceInfo> result = new List<ServiceInfo>();

                foreach (ServiceController serviceController in serviceControllers)
                {
                    var info = new ServiceInfo
                    {
                        MachineName = machineName,
                        Name = serviceController.ServiceName,
                        Status = serviceController.Status.ToString()
                    };
                    result.Add(info);
                }

                ObjectQuery wmiQuery = new ObjectQuery("SELECT * FROM Win32_Service");
                ManagementScope scope1 = new ManagementScope($"\\\\{machineName}\\root\\cimv2");
                scope1.Connect();
                var scope = scope1;
                var searcher = new ManagementObjectSearcher(scope, wmiQuery);
                var results = searcher.Get();
                foreach (ManagementObject moService in results)
                {
                    string name = moService["Name"].ToString();
                    string startupType = moService["StartMode"].ToString();
                    var serviceInfo = result.FirstOrDefault(i => i.Name == name);
                    if (serviceInfo != null)
                    {
                        serviceInfo.StartupType = startupType == "Auto" ? "Automatic" : startupType;
                    }
                }
                return result;
            });
        }

        public Task StartService(ServiceInfo service)
        {
            return Task.Run(() =>
            {
                ServiceController.GetServices(service.MachineName)
                    .FirstOrDefault(s => s.ServiceName == service.Name)
                    ?.Start();
            });
        }

        public Task StopService(ServiceInfo service)
        {
            return Task.Run(() =>
            {
                ServiceController.GetServices(service.MachineName)
                    .FirstOrDefault(s => s.ServiceName == service.Name)
                    ?.Stop();
            });
        }

        public Task ChangeStartupType(ServiceInfo service, string startupType)
        {
            return Task.Run(() =>
            {
                string networkPath = $"\\\\{service.MachineName}\\root\\cimv2";
                string objPath = $"Win32_Service.Name='{service.Name}'";
                using (var moService = new ManagementObject(networkPath, objPath, null))
                {
                    moService.InvokeMethod("ChangeStartMode", new object[] { startupType });
                }
            });
        }
    }
}