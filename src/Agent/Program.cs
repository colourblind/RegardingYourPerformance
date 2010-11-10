using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace RegardingYourPerformance.Agent
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-i" || args[0] == "-u")
                {
                    ServiceInstaller serviceInstaller = new ServiceInstaller();
                    serviceInstaller.ServiceName = "Performance.Agent.Service";
                    serviceInstaller.StartType = ServiceStartMode.Automatic;
                    serviceInstaller.DisplayName = "Colourblind Performance Agent";
                    serviceInstaller.Description = "Agent for the Colourblind performance monitor";

                    ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
                    processInstaller.Account = ServiceAccount.LocalSystem;
                    processInstaller.Username = null;
                    processInstaller.Password = null;

                    TransactedInstaller installer = new TransactedInstaller();
                    installer.Installers.Add(processInstaller);
                    installer.Installers.Add(serviceInstaller);
                    installer.Context = new InstallContext("install.log", null);
                    installer.Context.Parameters.Add("assemblypath", Assembly.GetCallingAssembly().Location);

                    if (args[0] == "-i")
                        installer.Install(new Hashtable());
                    else if (args[0] == "-u")
                        installer.Uninstall(null);
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
		        { 
			        new Service() 
		        };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
