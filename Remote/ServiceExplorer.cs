using System;
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
                    uint res = (uint)moService.InvokeMethod("ChangeStartMode", new object[] { startupType });
                    if (changeStartModeErrors.ContainsKey(res))
                    {
                        throw new InvalidOperationException(changeStartModeErrors[res]);
                    }
                }
            });
        }

        private readonly Dictionary<uint, string> changeStartModeErrors = new Dictionary<uint, string>
        {
            {1, @"Not Supported
The request is not supported."},
            {2, @"Access Denied
The user did not have the necessary access."},
            {3,@"Dependent Services Running
The service cannot be stopped because other services that are running are dependent on it."},
            {4,@"Invalid Service Control
The requested control code is not valid, or it is unacceptable to the service."},
            {5,@"Service Cannot Accept Control
The requested control code cannot be sent to the service because the state of the service (Win32_BaseService.State property) is equal to 0, 1, or 2. "},
            {6,@"Service Not Active
The service has not been started."},
            {7,@"Service Request Timeout
The service did not respond to the start request in a timely fashion."},
            {8,@"Unknown Failure
Unknown failure when starting the service."},
            {9,@"Path Not Found
The directory path to the service executable file was not found."},
            {10,@"Service Already Running
The service is already running."},
            {11,@"Service Database Locked
The database to add a new service is locked."},
            {12,@"Service Dependency Deleted
A dependency this service relies on has been removed from the system."},
            {13,@"Service Dependency Failure
The service failed to find the service needed from a dependent service."},
            {14,@"Service Disabled
The service has been disabled from the system."},
            {15,@"Service Logon Failed
The service does not have the correct authentication to run on the system."},
            {16,@"Service Marked For Deletion
This service is being removed from the system."},
            {17,@"Service No Thread
The service has no execution thread."},
            {18,@"Status Circular Dependency
The service has circular dependencies when it starts."},
            {19,@"Status Duplicate Name
A service is running under the same name."},
            {20,@"Status Invalid Name
The service name has invalid characters."},
            {21,@"Status Invalid Parameter
Invalid parameters have been passed to the service."},
            {22,@"Status Invalid Service Account
The account under which this service runs is either invalid or lacks the permissions to run the service."},
            {23,@"Status Service Exists
The service exists in the database of services available from the system."},
            {24,@"Service Already Paused
The service is currently paused in the system."}
        };
    }
}